using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dissertation.Modeling.Helpers
{
    public class BoundedTaskScheduler : TaskScheduler
    {
        public BoundedTaskScheduler(int maxConcurrentTasks)
        {
            if (maxConcurrentTasks < 1)
                throw new ArgumentOutOfRangeException("maxConcurrentTasks < 1");

            MaxConcurrentTasks = maxConcurrentTasks;
        }

        // cache thread pool callback
        readonly WaitCallback ThreadPoolCallback = ProcessQueuedTasks;

        // Queue representation
        Task[] Tasks = Arrays.Empty<Task>();
        int TasksHead;
        int TasksTail;
        int TasksCount;

        // current concurrency
        int ConcurrentTasksCount;

        // Stop state
        Task StopTask;

        /// <summary>
        /// Maximum number of concurrent thread pool tasks allocated by
        /// this scheduler
        /// </summary>
        public int MaxConcurrentTasks { get; private set; }

        /// <summary>
        /// Clear all tasks, prevent new tasks from being queued and
        /// return a task that completes when already running task complete.
        /// </summary>
        public Task Stop()
        {
            lock (this)
            {
                // Prevent new tasks from being executed by setting stop task.
                // Start a sentinel task that will not be queued in a normal way.
                // This is necessary to assign scheduler to the task
                // which is required for the completion of the task.
                if (ConcurrentTasksCount > 0)
                {
                    StopTask = new Task(self => { }, this);
                    StopTask.Start(this);
                }
                else
                {
                    StopTask = Task.CompletedTask;
                }

                // Clear queue representation
                try { }
                finally
                {
                    // No thread aborts in finally 
                    TasksHead = TasksTail = 0;
                    TasksCount = 0;
                    Array.Clear(Tasks, 0, Tasks.Length);
                }

                return StopTask;
            }

        }

        /// <summary>
        /// Resume executing tasks after <see cref="Stop"/>
        /// </summary>
        public void Resume()
        {
            lock (this)
            {
                if (StopTask != null && StopTask.IsCompleted)
                {
                    StopTask = null;
                }
            }
        }

        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough 
            // delegates currently queued or running to process tasks, schedule another. 
            var queueWorkItem = false;

            lock (this)
            {
                if (StopTask == null)
                {
                    // Thead aborts are not allowed in finally
                    try { }
                    finally
                    {
                        Enqueue(task);
                        if (ConcurrentTasksCount < MaxConcurrentTasks)
                        {
                            ++ConcurrentTasksCount;
                            queueWorkItem = true;
                        }
                    }
                }
            }

            if (queueWorkItem)
                ThreadPool.UnsafeQueueUserWorkItem(ThreadPoolCallback, this);
        }

        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        static bool CurrentThreadIsProcessingTasks;

        static void ProcessQueuedTasks(object state)
        {
            try
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                CurrentThreadIsProcessingTasks = true;

                ((BoundedTaskScheduler)state).ProcessQueuedTasks();
            }
            finally
            {
                // We're done processing items on the current thread
                CurrentThreadIsProcessingTasks = false;
            }
        }

        void ProcessQueuedTasks()
        {
            while (true)
            {
                Task item;
                lock (this)
                {
                    // When there are no more items to be processed,
                    // note that we're done processing, and get out
                    // Before getting out check pending stop and execute it.
                    // After stop is pending, TaskCount is always 0.
                    if (TasksCount == 0)
                    {
                        --ConcurrentTasksCount;
                        if (StopTask != null && ConcurrentTasksCount == 0 && !StopTask.IsCompleted)
                            TryExecuteTask(StopTask);
                        break;
                    }
                    // No thread aborts allowed in finally
                    try { }
                    finally
                    {
                        item = Dequeue();
                    }
                }
                // Execute the task we pulled out of the queue
                TryExecuteTask(item);
            }
        }

        // Attempts to execute the specified task on the current thread. 
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!CurrentThreadIsProcessingTasks)
                return false;

            // If the task was previously queued, remove it from the queue and try to run
            if (taskWasPreviouslyQueued && !TryDequeue(task))
                return false;

            return TryExecuteTask(task);
        }

        ///<summary>
        /// Attempt to remove a previously scheduled task from the scheduler. 
        /// </summary>
        /// <remarks>
        /// Typically happens on task cancelation
        /// </remarks>
        protected sealed override bool TryDequeue(Task task)
        {
            lock (this)
            {
                if (TasksCount == 0)
                    return false;

                if (TasksHead < TasksTail)
                {
                    var index = Array.IndexOf(Tasks, task, TasksHead, TasksCount);
                    if (index < 0)
                        return false;

                    if (index != TasksHead)
                        Array.Copy(Tasks, TasksHead, Tasks, TasksHead + 1, index - TasksHead);

                    Tasks[TasksHead] = default(Task);
                    TasksHead = (TasksHead + 1) % Tasks.Length;
                }
                else
                {
                    var index = Array.IndexOf(Tasks, task, TasksHead, Tasks.Length - TasksHead);
                    if (index >= 0)
                    {
                        if (index != TasksHead)
                            Array.Copy(Tasks, TasksHead, Tasks, TasksHead + 1, index - TasksHead);

                        Tasks[TasksHead] = default(Task);
                        TasksHead = (TasksHead + 1) % Tasks.Length;
                    }
                    else
                    {
                        index = Array.IndexOf(Tasks, task, 0, TasksTail);
                        if (index < 0)
                            return false;

                        var n = TasksTail - index - 1;
                        if (n > 0)
                            Array.Copy(Tasks, index + 1, Tasks, index, n);

                        Tasks[--TasksTail] = default(Task);
                    }
                }

                --TasksCount;

                return true;
            }
        }

        Task Dequeue()
        {
            var result = Tasks[TasksHead];
            Tasks[TasksHead] = default(Task);
            TasksHead = (TasksHead + 1) % Tasks.Length;
            --TasksCount;
            return result;
        }

        void Enqueue(Task item)
        {
            if (TasksCount == Tasks.Length)
            {
                int capacity = Math.Max(Tasks.Length * 2, Tasks.Length + 4);
                Task[] arr = new Task[capacity];
                if (TasksCount > 0)
                {
                    if (TasksHead < TasksTail)
                    {
                        Array.Copy(Tasks, TasksHead, arr, 0, TasksCount);
                    }
                    else
                    {
                        Array.Copy(Tasks, TasksHead, arr, 0, Tasks.Length - TasksHead);
                        Array.Copy(Tasks, 0, arr, Tasks.Length - TasksHead, TasksTail);
                    }
                }
                Tasks = arr;
                TasksHead = 0;
                TasksTail = TasksCount;
            }

            Tasks[TasksTail] = item;
            TasksTail = (TasksTail + 1) % Tasks.Length;
            ++TasksCount;
        }

        /// <summary>
        /// Gets an enumerable of the tasks currently scheduled on this scheduler.  
        /// </summary>
        /// <returns>
        /// Fresh array containing enqueued tasks
        /// </returns>
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            lock (this)
            {
                if (TasksCount == 0)
                    return Arrays.Empty<Task>();

                var result = new Task[TasksCount];

                if (TasksHead < TasksTail)
                {
                    Array.Copy(Tasks, TasksHead, result, 0, TasksCount);
                }
                else
                {
                    int n = Tasks.Length - TasksHead;
                    Array.Copy(Tasks, TasksHead, result, 0, n);
                    Array.Copy(Tasks, 0, result, n, TasksTail);
                }
                return result;
            }
        }
    }

    public static class Arrays
    {
        public static T[] Empty<T>()
        {
            return (T[])Enumerable.Empty<T>();
        }
    }
}
