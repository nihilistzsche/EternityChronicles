using DragonMUD.Network;
using DragonMUD.StateMachine;
using DragonMUD.StateMachine.Workflows;
using Dynamitey;

namespace DragonMUD.Interpreters
{
    public class BasicInterpreter : BaseInterpreter
    {
        public BasicInterpreter() : base()
        {
            
        }
        
        protected void Interpret(ConnectionCoordinator coordinator, InputEventArgs args, IState oldState)
        {
            var newState = coordinator["current-state"] as IState;
            var workflow = coordinator["current-workflow"] as Workflow;
            var step = coordinator["current-workflow-step"] as WorkflowStep;

            if (newState != oldState)
            {
                if (workflow != null)
                {
					if (workflow.HasStepForState(newState.GetType()))
                    {
						workflow.SetWorkflowToStepForCoordinator(newState.GetType(), coordinator);
                    }
                    else
                    {
                        workflow.AdvanceWorkflowForCoordinator(coordinator);
                        
                    }
                    step = coordinator["current-workflow-step"] as WorkflowStep;
                    if (step.NextStep == null)
                    {
                        coordinator["current-workflow"] = null;
                    }
					newState = Dynamic.InvokeConstructor(step.StateType);
					coordinator["current-state"] = newState;
                }
                var interpreter = newState.Interpreter ?? new BasicInterpreter();
                coordinator["current-interpreter"] = interpreter;
            }
            if (!coordinator.IsFlagSet("no-message"))
            {
                var state = coordinator["current-state"] as IState;
                state.SendSoftRebootMessage(coordinator);
            }
            else
            {
                coordinator.ClearFlag("no-message");
            }
        }
        
        public override void Interpret(ConnectionCoordinator coordinator, InputEventArgs args)
        {
            coordinator.ClearFlag("softreboot-displayed");
            var state = coordinator["current-state"] as IState;
            state.Process(coordinator, args.Input);
            Interpret(coordinator, args, state);
        }
        
    }
}