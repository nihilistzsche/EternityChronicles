// Workflow.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using CSLog;
using DragonMUD.Network;
using Dynamitey;

namespace DragonMUD.StateMachine.Workflows
{
    public class Workflow
    {
        public readonly Dictionary<string, WorkflowStep> Steps = new();

        public WorkflowStep FirstStep { get; private set; }

        protected static bool CheckState(Type T)
        {
            return T.GetInterface("IState") != null && T.GetConstructor(new Type[] { }) != null;
        }

        public void DebugPrintWorkflow(Type T)
        {
            var step = Steps[T.Name];
            var zstep = 1;
            while (step != null)
            {
                Log.LogMessage("dragonmud", LogLevel.Debug, $"Step #{zstep}: {step.StateType.Name}");
                zstep++;
                step = step.NextStep;
            }
        }

        protected void AddStep(Type T)
        {
            if (!CheckState(T) || Steps.ContainsKey(T.Name))
                return;

            Steps[T.Name] = new WorkflowStep(T);
        }

        public void SetNextStep(Type T, Type K)
        {
            if (!CheckState(T) || !CheckState(K))
                return;

            if (!Steps.ContainsKey(T.Name)) return;

            var step = Steps[T.Name];

            if (!Steps.ContainsKey(K.Name)) AddStep(K);

            step.NextStep = Steps[K.Name];
        }

        public void InsertStepBefore(Type T, Type K)
        {
            if (!CheckState(T) || !CheckState(K))
                return;

            if (!Steps.ContainsKey(T.Name))
                return;

            var step = Steps[T.Name];
            var oldBeforeStep = (from xstep in Steps.Values where xstep.NextStep == step select xstep).FirstOrDefault();

            if (!Steps.ContainsKey(K.Name))
                AddStep(K);

            var newStep = Steps[K.Name];
            newStep.NextStep = step;
            if (oldBeforeStep != null) oldBeforeStep.NextStep = newStep;
        }

        public void InsertStepAfter(Type T, Type K)
        {
            if (!CheckState(T) || !CheckState(K))
                return;

            if (!Steps.ContainsKey(T.Name))
                return;

            var step = Steps[T.Name];

            if (!Steps.ContainsKey(K.Name))
                AddStep(K);

            var newStep = Steps[K.Name];
            newStep.NextStep = step.NextStep;
            step.NextStep = newStep;
        }

        public void Remove(Type T)
        {
            if (!Steps.ContainsKey(T.Name))
                return;

            var step = Steps[T.Name];

            var oldBeforeStep = (from xstep in Steps.Values where xstep.NextStep == step select xstep).FirstOrDefault();

            if (oldBeforeStep != null) oldBeforeStep.NextStep = step.NextStep;

            Steps.Remove(T.Name);
        }

        public void StartWorkflowForCoordinator(ConnectionCoordinator coordinator)
        {
            var step = FirstStep;
            coordinator["current-workflow-step"] = step;
            var state = Dynamic.InvokeConstructor(step.StateType);
            coordinator["current-state"] = state;
            coordinator["current-workflow"] = this;
            state.SendSoftRebootMessage(coordinator);
        }

        public void AdvanceWorkflowForCoordinator(ConnectionCoordinator coordinator)
        {
            if (coordinator["current-workflow-step"] is not WorkflowStep step)
                return;
            step = step.NextStep;
            if (step == null)
                return;
            var state = Dynamic.InvokeConstructor(step.StateType);
            coordinator["current-state"] = state;
            coordinator["current-workflow-step"] = step;
        }

        public void SetWorkflowToStepForCoordinator(Type T, ConnectionCoordinator coordinator)
        {
            var step = Steps[T.Name];

            if (step == null)
                return;

            coordinator["current-workflow-step"] = step;
            var state = Dynamic.InvokeConstructor(step.StateType);
            coordinator["current-state"] = state;
        }

        public bool HasStepForState(Type T)
        {
            return Steps.ContainsKey(T.Name);
        }

        public static Workflow CreateWorkForSteps(params Type[] steps)
        {
            var xsteps = steps.ToList();

            var ok = true;
            xsteps.ForEach(obj =>
                           {
                               if (ok) ok = CheckState(obj);
                           });
            if (!ok)
                return null;

            var firstStep = xsteps.First();
            xsteps.RemoveAt(0);

            var wf = new Workflow();

            wf.AddStep(firstStep);

            wf.FirstStep = wf.Steps[firstStep.Name];

            var curState = firstStep;

            xsteps.ForEach(state =>
                           {
                               wf.InsertStepAfter(curState, state);
                               curState = state;
                           });

            return wf;
        }
    }
}