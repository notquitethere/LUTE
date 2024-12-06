using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(ConditionalEventHandler), true)]
    public class ConditionalEventHandlerEditor : EventHandlerEditor
    {
        protected Texture2D addIcon;
        protected Texture2D removeIcon;
        protected Texture2D duplicateIcon;

        private OrderListAdapter orderListAdapter;
        private SerializedProperty fireMode;
        private SerializedProperty orderListProp;
        private Rect lastEventPopupPos, lastCMDpopupPos;

        protected virtual void OnEnable()
        {
            //this appears to happen when leaving playmode
            try
            {
                if (serializedObject == null)
                    return;
            }
            catch (Exception)
            {
                return;
            }

            addIcon = LogaEditorResources.Add;
            removeIcon = LogaEditorResources.Remove;
            duplicateIcon = LogaEditorResources.Duplicate;

            fireMode = serializedObject.FindProperty("fireMode");
            orderListProp = serializedObject.FindProperty("conditions");
            if (orderListAdapter == null)
            {
                Debug.Log("Creating new OrderListAdapter");
                orderListAdapter = new OrderListAdapter(null, orderListProp, null, target as ConditionalEventHandler);
            }
        }

        protected override void DrawProperties()
        {
            serializedObject.Update();

            var handler = target as ConditionalEventHandler;

            EditorGUILayout.PropertyField(fireMode, new GUIContent("Fire Mode", "When to check the conditions. Start happens once, update continously checks."));

            DrawHandlerToolBar();

            handler.UpdateIndentLevels();

            //BUG: the list cannot be reordered in the inspector - see the debugs for the LONG ASS fix

            //also update indent levels in awake and set execution info (see node awake for this)
            //^not massively important but worth doing (just like below)

            //the condition sanity check requires this too - you need to check if we are using event handler rather than node
            //and set a new method on the handler (GetPreviousActiveOrderIndent) to get the indent level of the previous active order
            //there is a couple of other items to check on the condition sanity check too - however because we know the prior order is a condition we do not need to worry too much about these last few points

            orderListAdapter.DrawOrderList();

            for (int i = orderListProp.arraySize - 1; i >= 0; --i)
            {
                SerializedProperty orderProperty = orderListProp.GetArrayElementAtIndex(i);
                if (orderProperty.objectReferenceValue == null)
                {
                    orderListProp.DeleteArrayElementAtIndex(i);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawHandlerToolBar()
        {
            var handler = target as ConditionalEventHandler;

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            //using false to prevent forcing a longer row than will fit on smallest inspector -- for now we only want add button
            var pos = EditorGUILayout.GetControlRect(false, 0, EditorStyles.objectField);
            if (pos.x != 0)
            {
                lastCMDpopupPos = pos;
                lastCMDpopupPos.x += EditorGUIUtility.labelWidth;
                lastCMDpopupPos.y += EditorGUIUtility.singleLineHeight * 2;
            }

            if (GUILayout.Button(addIcon))
            {
                OrderSelectorPopupWindowContent.AddOrderCallBack(typeof(If), handler);
            }
            if (GUILayout.Button(duplicateIcon))
            {
                CopyOrder();
                PasteOrder();
            }
            if (GUILayout.Button(removeIcon))
            {
                DeleteCondition();
            }

            GUILayout.EndHorizontal();
        }

        protected void CopyOrder()
        {
            var handler = target as ConditionalEventHandler;
            var engine = handler.ParentNode.GetEngine();

            if (engine == null || target == null)
            {
                return;
            }

            OrderCopyBuffer orderCopyBuffer = OrderCopyBuffer.GetInstance();
            orderCopyBuffer.Clear();

            //go through all orders to deterimine which ones need copying
            foreach (Order order in handler.Conditions)
            {
                if (engine.SelectedOrders.Contains(order))
                {
                    var type = order.GetType();
                    Order newOrder = Undo.AddComponent(orderCopyBuffer.gameObject, type) as Order;
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    foreach (var field in fields)
                    {
                        //copy all public fields
                        bool copy = field.IsPublic;
                        //copy non public fields that have the SerializeField attribute
                        var attributes = field.GetCustomAttributes(typeof(SerializeField), true);
                        if (attributes.Length > 0)
                        {
                            copy = true;
                        }

                        if (copy)
                        {
                            field.SetValue(newOrder, field.GetValue(order));
                        }
                    }
                }
            }
        }

        protected void PasteOrder()
        {
            var handler = target as ConditionalEventHandler;
            var engine = handler.ParentNode.GetEngine();

            if (engine == null || target == null)
            {
                return;
            }

            //get the index to paste the orders at
            OrderCopyBuffer orderCopyBuffer = OrderCopyBuffer.GetInstance();

            //if there is no selected order, paste at the end of the list
            int pasteIndex = handler.Conditions.Count;
            //if there is a selected order, paste after the last selected order
            if (engine.SelectedOrders.Count > 0)
            {
                for (int i = 0; i < handler.Conditions.Count; i++)
                {
                    Order order = handler.Conditions[i];

                    foreach (Order selectedOrder in engine.SelectedOrders)
                    {
                        if (order == selectedOrder)
                        {
                            pasteIndex = i + 1;
                        }
                    }
                }
            }

            //go through all orders in the copy buffer and paste them
            foreach (Order order in orderCopyBuffer.GetOrders())
            {
                // Using the Editor copy / paste functionality instead of reflection
                // because this does a deep copy
                if (ComponentUtility.CopyComponent(order))
                {
                    // Paste the component as a new component
                    if (ComponentUtility.PasteComponentAsNew(engine.gameObject))
                    {
                        // Get the newly pasted component
                        Order[] orders = engine.GetComponents<Order>();
                        // Get the last pasted component
                        Order pastedOrder = orders.Last<Order>();
                        // Set the item id to a new unique value
                        if (pastedOrder != null)
                        {
                            pastedOrder.ItemId = engine.NextItemId();
                            handler.Conditions.Insert(pasteIndex++, pastedOrder as If);
                        }
                    }

                    // This stops the user pasting the order manually into another game object
                    ComponentUtility.CopyComponent(engine.transform);
                }
            }

            // Because this is an async call, we need to force prefab instances to record changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(handler);

            Repaint();
        }

        protected void DeleteCondition()
        {
            var handler = target as ConditionalEventHandler;
            var engine = handler.ParentNode.GetEngine();

            if (engine == null || target == null)
            {
                return;
            }

            //go through all orders to determine which ones need deleting
            int lastSelectedIndex = 0;
            for (int i = handler.Conditions.Count - 1; i >= 0; i--)
            {
                Order order = handler.Conditions[i];
                foreach (Order selectedOrder in engine.SelectedOrders)
                {
                    if (order == selectedOrder)
                    {
                        //remove the order from the list - important to do this to ensure undo works
                        Undo.DestroyObjectImmediate(order);

                        Undo.RecordObject(handler.ParentNode, "DeleteOrder");
                        handler.Conditions.RemoveAt(i);

                        lastSelectedIndex = i;

                        break;
                    }
                }
            }
        }
    }
}
