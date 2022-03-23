// BasicInterpreter.cs in EternityChronicles/DragonMUD
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using DragonMUD.Network;
using DragonMUD.StateMachine;
using DragonMUD.StateMachine.Workflows;
using Dynamitey;

namespace DragonMUD.Interpreters
{
    public class BasicInterpreter : BaseInterpreter
    {
        protected void Interpret(ConnectionCoordinator coordinator, InputEventArgs args, IState oldState)
        {
            var newState = coordinator["current-state"] as IState;

            if (newState != oldState)
            {
                if (coordinator["current-workflow"] is Workflow workflow)
                {
                    if (workflow.HasStepForState(newState.GetType()))
                        workflow.SetWorkflowToStepForCoordinator(newState.GetType(), coordinator);
                    else
                        workflow.AdvanceWorkflowForCoordinator(coordinator);
                    if ((coordinator["current-workflow-step"] as WorkflowStep).NextStep == null)
                        coordinator["current-workflow"] = null;
                    newState = Dynamic.InvokeConstructor((coordinator["current-workflow-step"] as WorkflowStep)
                                                         .StateType);
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