using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [EventHandlerInfo("Conditional",
                  "Condition List",
                  "Executes when the list of conditions has been met")]
    [AddComponentMenu("")]
    public class ConditionalEventHandler : EventHandler
    {
        public enum FireMode
        {
            Start,
            Update
        }

        [Tooltip("When to check the conditions. Start happens once, update continously checks.")]
        [SerializeField] protected FireMode fireMode = FireMode.Start;
        [Tooltip("The list of conditions to check.")]
        [SerializeField] protected List<If> conditions = new List<If>();

        private bool isComplete;

        public List<If> Conditions
        {
            get { return conditions; }
        }

        public virtual void UpdateIndentLevels()
        {
            int indentLevel = 0;
            for (int i = 0; i < conditions.Count; i++)
            {
                var order = conditions[i];
                if (order == null)
                {
                    continue;
                }
                if (order.CloseNode())
                {
                    indentLevel--;
                }
                // Negative indent level is not permitted
                indentLevel = System.Math.Max(indentLevel, 0);
                order.IndentLevel = indentLevel;
                if (order.OpenNode())
                {
                    indentLevel++;
                }
            }
        }

        protected virtual void Start()
        {
            if (fireMode == FireMode.Start)
                CheckConditions();
        }

        protected virtual void Update()
        {
            if (fireMode == FireMode.Update)
                CheckConditions();
        }

        protected virtual void CheckConditions()
        {
            if (conditions.Count == 0)
            {
                ExecuteNode();
                return;
            }

            foreach (If condition in conditions)
            {
                if (!condition.EvaluateConditions())
                {
                    isComplete = false;
                    break;
                }
                else
                {
                    isComplete = true;
                }
            }
            if (isComplete)
            {
                ExecuteNode();
            }
        }


        public override string GetSummary()
        {
            return "This node will execute upon all conditions being met.";
        }
    }
}