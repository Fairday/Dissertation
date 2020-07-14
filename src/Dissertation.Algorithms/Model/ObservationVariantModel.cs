using Dissertation.Algorithms.Algorithms.Model;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.Model
{
    public enum NodeType
    {
        Upstream, Downstream, Vertex
    }

    public class Node
    {
        public SatelliteOld Sattelite { get; }
        public int Number { get; }
        public NodeType NodeType { get; }

        public Node(int number, NodeType nodeType, SatelliteOld sattelite)
        {
            Number = number;
            NodeType = nodeType;
            Sattelite = sattelite;
        }
    }

    public class ObservationVariant
    {
        public Node[] NodeSequential { get; }

        public string Variant { get; internal set; }

        public ObservationVariant(Node[] nodes, string variant)
        {
            Variant = variant;
            NodeSequential = nodes;
        }
    }

    public class ObservationVariantTimeInfo
    {
        public NodeTime[] NodeTimes { get; }
        public NodeTime[] SortedNodeTimes { get; }

        public double ViewPeriodicity { get; }
        public ObservationVariant ObservationVariant { get; }

        public ObservationVariantTimeInfo(ObservationVariant observationVariant, double lattitude, SatelliteSystemTimeShift[] satelliteSystemTimeShifts)
        {
            var nodeTimeRow = new List<NodeTime>();

            foreach (var node in observationVariant.NodeSequential)
            {
                var nodeTime = new NodeTime(node, node.Sattelite, lattitude, satelliteSystemTimeShifts);
                nodeTimeRow.Add(nodeTime);
            }

            double? prevTime = null;
            var maxInterval = 0d;

            //foreach (var node in observationVariant.NodeSequential)
            //{
            //    var nodeTime = new NodeTime(node, node.Sattelite, lattitude, satelliteSystemTimeShifts);
            //    if (prevTime == null)
            //    {
            //        maxInterval = nodeTime.T;
            //    }
            //    else if (prevTime != null)
            //    {
            //        var interval = nodeTime.T - prevTime;
            //        if (interval > maxInterval)
            //            maxInterval = (double)interval;
            //    }
            //    nodeTimeRow.Add(nodeTime);
            //    prevTime = nodeTime.T;
            //}

            foreach (var nodeTime in nodeTimeRow.OrderBy(n => n.T))
            {
                if (prevTime == null)
                {
                    maxInterval = nodeTime.T;
                }
                else if (prevTime != null)
                {
                    var interval = nodeTime.T - prevTime;
                    if (interval > maxInterval)
                        maxInterval = (double)interval;
                }
                prevTime = nodeTime.T;
            }

            if (maxInterval == 0)
                ViewPeriodicity = 24 * 60 * 60;
            else
                ViewPeriodicity = maxInterval;

            NodeTimes = nodeTimeRow.ToArray();
            SortedNodeTimes = NodeTimes.OrderBy(n => n.T).ToArray();
            ObservationVariant = observationVariant;
        }
    }
}
