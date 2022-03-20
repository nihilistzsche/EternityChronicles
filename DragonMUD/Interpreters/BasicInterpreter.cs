// BasicInterpreter.cs
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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