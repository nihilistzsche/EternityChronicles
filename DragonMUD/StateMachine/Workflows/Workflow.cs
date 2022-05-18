// Workflow.cs in EternityChronicles/DragonMUD
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

        public void SetNextStep(Type T, Type k)
        {
            if (!CheckState(T) || !CheckState(k))
                return;

            if (!Steps.ContainsKey(T.Name)) return;

            var step = Steps[T.Name];

            if (!Steps.ContainsKey(k.Name)) AddStep(k);

            step.NextStep = Steps[k.Name];
        }

        public void InsertStepBefore(Type T, Type k)
        {
            if (!CheckState(T) || !CheckState(k))
                return;

            if (!Steps.ContainsKey(T.Name))
                return;

            var step = Steps[T.Name];
            var oldBeforeStep = (from xstep in Steps.Values where xstep.NextStep == step select xstep).FirstOrDefault();

            if (!Steps.ContainsKey(k.Name))
                AddStep(k);

            var newStep = Steps[k.Name];
            newStep.NextStep = step;
            if (oldBeforeStep != null) oldBeforeStep.NextStep = newStep;
        }

        public void InsertStepAfter(Type T, Type k)
        {
            if (!CheckState(T) || !CheckState(k))
                return;

            if (!Steps.ContainsKey(T.Name))
                return;

            var step = Steps[T.Name];

            if (!Steps.ContainsKey(k.Name))
                AddStep(k);

            var newStep = Steps[k.Name];
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