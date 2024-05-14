
using UnityEngine;
using UnityEditor;
using System;
using UnityEditorInternal;


public class OrderListAdapter
{
    public void DrawOrderList()
    {
        if (summaryStyle == null)
        {
            summaryStyle = new GUIStyle();
            summaryStyle.fontSize = 10;
            summaryStyle.padding.top += 5;
            summaryStyle.richText = true;
            summaryStyle.wordWrap = false;
            summaryStyle.clipping = TextClipping.Clip;
        }

        if (orderLabelStyle == null)
        {
            orderLabelStyle = new GUIStyle(GUI.skin.label);
            orderLabelStyle.fontStyle = FontStyle.Bold;
            orderLabelStyle.normal.background = LogaEditorResources.OrderBackground;
            orderLabelStyle.normal.textColor = Color.black;
            int borderSize = 5;
            orderLabelStyle.border.top = borderSize;
            orderLabelStyle.border.bottom = borderSize;
            orderLabelStyle.border.left = borderSize;
            orderLabelStyle.border.right = borderSize;
            orderLabelStyle.alignment = TextAnchor.MiddleLeft;
            orderLabelStyle.richText = true;
            orderLabelStyle.fontSize = 11;
            orderLabelStyle.padding.top -= 1;
            orderLabelStyle.alignment = TextAnchor.MiddleLeft;
        }

        if (node != null && node.OrderList.Count == 0 || group != null && group.OrderList.Count == 0)
        {
            EditorGUILayout.HelpBox("Press the add button to add an order to the list...", MessageType.Info);
        }
        else
        {
            EditorGUI.indentLevel++;
            list.DoLayoutList();
            EditorGUI.indentLevel--;
        }
    }

    protected SerializedProperty _arrayProp;
    protected ReorderableList list;
    protected Node node;
    protected Group group;
    protected GUIStyle summaryStyle, orderLabelStyle;


    public SerializedProperty this[int index]
    {
        get { return _arrayProp.GetArrayElementAtIndex(index); }
    }

    public SerializedProperty arrayProperty
    {
        get { return _arrayProp; }
    }

    public OrderListAdapter(Node _node, SerializedProperty arrayProp, Group group)
    {
        if (arrayProp == null)
            throw new ArgumentNullException("Array property was null");
        if (!arrayProp.isArray)
            throw new InvalidOperationException("Specified serialised propery is not an array");
        this._arrayProp = arrayProp;
        this.node = _node;
        this.group = group;

        list = new ReorderableList(arrayProp.serializedObject, arrayProp, true, true, false, false);
        list.drawHeaderCallback = DrawHeader;
        list.drawElementCallback = DrawItem;
        list.onSelectCallback = SelectChanged;
    }

    private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
    {
        // this simply draws the list item (not the actual content that will be edited) - if the inspector is closed then return
        if (rect.width < 0) return;

        Order order = this[index].objectReferenceValue as Order;

        if (order == null)
            return;

        OrderInfoAttribute orderInfo = OrderEditor.GetOrderInfo(order.GetType());
        if (orderInfo == null)
            return;

        var engine = (BasicFlowEngine)order.GetEngine();
        if (engine == null)
            return;

        bool isComment = node.GetType() == typeof(Comment);
        // bool isLabel = node.GetType() == typeof(Label);

        string summary = order.GetSummary();
        if (summary == null)
            summary = "";
        else
            summary = summary.Replace("\n", "").Replace("\r", "");

        if (summary.StartsWith("Error:"))
        {
            summary = "<color=red> " + summary + "</color>";
        }

        if (isComment)
        {
            summary = "<b> " + summary + "</b>";
        }
        else
        {
            summary = "<i>" + summary + "</i>";
        }

        bool orderIsSelected = false;
        foreach (Order selectedCOrder in engine.SelectedOrders)
        {
            if (selectedCOrder == order)
            {
                orderIsSelected = true;
                break;
            }
        }

        string orderName = orderInfo.OrderName;

        float indentSize = 20;
        for (int i = 0; i < order.IndentLevel; ++i)
        {
            Rect indentRect = rect;
            indentRect.x += i * indentSize;// - 21;
            indentRect.width = indentSize + 1;
            indentRect.y -= 2;
            indentRect.height += 5;
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            GUI.Box(indentRect, "", orderLabelStyle);
        }

        float orderNameWidth = Mathf.Max(orderLabelStyle.CalcSize(new GUIContent(orderName)).x, 90f);
        float indentWidth = order.IndentLevel * indentSize;

        Rect orderLabelRect = rect;
        orderLabelRect.x += indentWidth;// - 21;
        orderLabelRect.y -= 2;
        orderLabelRect.width -= (indentSize * order.IndentLevel);// - 22);
        orderLabelRect.height += 5;

        Color orderLabelColor = Color.white;
        if (engine.ColourOrders)
        {
            orderLabelColor = order.GetButtonColour();
        }

        if (orderIsSelected)
        {
            orderLabelColor = Color.green;
        }
        else if (!order.enabled)
        {
            orderLabelColor = Color.grey;
        }

        GUI.backgroundColor = orderLabelColor;

        if (isComment)
        {
            GUI.Label(orderLabelRect, "", orderLabelStyle);
        }
        else
        {
            string orderNameLabel;
            if (engine.ShowLineNumbers)
            {
                orderNameLabel = order.OrderIndex.ToString() + ": " + orderName;
            }
            else
            {
                orderNameLabel = orderName;
            }

            GUI.Label(orderLabelRect, orderNameLabel, orderLabelStyle);
        }

        //you then want to handle mouse clicks on the order in the list - adding it if lclick only then removing if ctrl + lclick
        //you can set your label colour of each order depending if it is selected or not

        if(order.ExecutingIconTimer > Time.realtimeSinceStartup)
        {
            Rect iconRect = new Rect(orderLabelRect);
            iconRect.x += iconRect.width - orderLabelRect.width - 20;
            iconRect.width = 20;
            iconRect.height = 20;

            Color storeColor = GUI.color;

            float alpha = (order.ExecutingIconTimer - Time.realtimeSinceStartup) / LogaConstants.ExecutingIconFadeTime;
            alpha = Mathf.Clamp01(alpha);

            GUI.color = new Color (1, 1, 1, alpha);
            GUI.Label(iconRect, LogaEditorResources.PlayBig, new GUIStyle());

            GUI.color = storeColor;
        }

        Rect summaryRect = new Rect(orderLabelRect);
        if (isComment)
        {
            summaryRect.x += 5;
        }
        else
        {
            summaryRect.x += orderNameWidth + 5;
            summaryRect.width -= orderNameWidth + 5;
        }

        GUI.Label(summaryRect, summary, summaryStyle);

        GUI.backgroundColor = Color.white;
    }

    private void DrawHeader(Rect rect)
    {
        if (rect.width < 0) return;
        EditorGUI.LabelField(rect, new GUIContent("Orders"), EditorStyles.boldLabel);
    }

    private void SelectChanged(ReorderableList list)
    {
        Order order = this[list.index].objectReferenceValue as Order;
        var engine = (BasicFlowEngine)order.GetEngine();
        NodeEditor.actionList.Add(delegate
        {
            engine.ClearSelectedOrders();
            engine.AddSelectedOrder(order);
        });
    }
}
