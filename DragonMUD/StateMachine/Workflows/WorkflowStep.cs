using System;

namespace DragonMUD.StateMachine.Workflows
{
    public class WorkflowStep
    {
        public WorkflowStep(Type myState)
        {
            StateType = myState;
        }

        public Type StateType { get; }

        public WorkflowStep NextStep { get; set; }
    }
}