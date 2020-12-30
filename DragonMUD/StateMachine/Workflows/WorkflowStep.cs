using System;

namespace DragonMUD.StateMachine.Workflows
{
    public class WorkflowStep
    {
        public Type StateType { get; }
        
        public WorkflowStep NextStep { get; set; }

        public WorkflowStep(Type myState)
        {
            StateType = myState;
        }
    }
}