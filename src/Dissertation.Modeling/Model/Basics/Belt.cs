using System;
using System.Collections;
using System.Collections.Generic;

namespace Dissertation.Modeling.Model.Basics
{
    public class Belt : IEnumerable<Angle>
    {
        public Belt(Angle start, Angle step, Angle stop)
        {
            if (stop.Grad < start.Grad && step != 0)
                throw new Exception("Stop angle can'not be less than Start angle");

            Start = start;
            Step = step;
            Stop = stop;

            CalculateAngles();
        }

        public Angle Start { get; }
        public Angle Step { get; }
        public Angle Stop { get; }
        public IEnumerable<Angle> Angles { get; private set; }

        public IEnumerator<Angle> GetEnumerator()
        {
            return Angles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Angles).GetEnumerator();
        }

        private void CalculateAngles()
        {
            if (Step.Grad == 0)
            {
                if (Start != Stop && Stop != 0)
                {
                    Angles = new Angle[]
                    {
                        Start, Stop
                    };
                }
                else if (Stop == 0)
                {
                    Angles = new Angle[]
                    {
                        Start
                    };
                }
            }
            else
            {
                var angles = new List<Angle>();
                var current = Start.Grad;
                while (current <= Stop.Grad)
                {
                    angles.Add(new Angle(current));
                    current += Step.Grad;
                }

                if (!angles.Contains(Stop))
                {
                    angles.Add(Stop);
                }

                Angles = angles.ToArray();
            }
        }
    }
}
