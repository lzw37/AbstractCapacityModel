using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class Train
    {
        public string ID;

        public List<Movement> CandidateMovementGroup = new List<Movement> ();

        public List<List<Movement>> PossibleMovementPath = new List<List<Movement>>();

        public List<Segment> SegmentList = new List<Segment> ();

        public void GenerateCandidateMovement()
        {
            foreach(Segment seg in SegmentList)
            {
                for(int t = 0; t<=Parameters.TimeHorizon-seg.RunningTime;t++)
                {
                    TrainSegmentMovement m = new TrainSegmentMovement()
                    {
                        ID = DataRepository.MovementList.Count.ToString(),
                        BindingTrain = this,
                        FromLocation = seg.FromStation,
                        ToLocation = seg.ToStation,
                        FromTime = t,
                        ToTime = t + seg.RunningTime,
                        OnSegment = seg,
                    };
                    DataRepository.MovementList.Add(m);
                    CandidateMovementGroup.Add(m);
                }
            }
        }


        public void GeneratePossibleMovementPath()
        {
            Stack<Node> NodeStack = new Stack<Node>();

            for (int t = 0; t <= Parameters.TimeHorizon - SegmentList[0].RunningTime; t++)
            {
                Node n = new Node()
                {
                    Segment = SegmentList[0],
                };
                NodeStack.Push(n);

                TrainSegmentMovement rArc = new TrainSegmentMovement()
                {
                    ID = DataRepository.MovementList.Count.ToString(),
                    FromLocation = SegmentList[0].FromStation,
                    ToLocation = SegmentList[0].ToStation,
                    FromTime = t,
                    ToTime = t + SegmentList[0].RunningTime
                };
                n.MovementList.Add(rArc);
                DataRepository.MovementList.Add(rArc);
                CandidateMovementGroup.Add(rArc);
            }

            while(NodeStack.Count > 0)
            {
                Node current = NodeStack.Pop();

                Segment seg = current.Segment;
                if(seg == SegmentList.Last())
                {
                    PossibleMovementPath.Add(current.MovementList);
                    continue;
                }

                Segment nextSeg = SegmentList[SegmentList.IndexOf(seg) + 1];

                for (int t = current.MovementList.Last().ToTime; t <= Parameters.TimeHorizon - nextSeg.RunningTime; t++)
                {
                    Node n = new Node()
                    {
                        Segment = nextSeg,
                        MovementList = new List<Movement>(current.MovementList),
                    };
                    NodeStack.Push(n);

                    TrainSegmentMovement m = new TrainSegmentMovement()
                    {
                        ID = DataRepository.MovementList.Count.ToString(),
                        FromLocation = nextSeg.FromStation,
                        ToLocation = nextSeg.ToStation,
                        FromTime = t,
                        ToTime = t + nextSeg.RunningTime
                    };
                    n.MovementList.Add(m);
                    DataRepository.MovementList.Add(m);
                    CandidateMovementGroup.Add(m);
                }
            }
        }

        public override string ToString()
        {
            return ID;
        }

        internal class Node
        {
            internal Segment Segment;

            internal List<Movement> MovementList = new List<Movement>();
        }
    }


}
