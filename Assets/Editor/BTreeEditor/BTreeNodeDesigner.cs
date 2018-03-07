﻿using System.Collections.Generic;
using BTreeFrame;
using UnityEngine;
using System;

namespace BTree.Editor
{
    public class BTreeNodeDesigner<T, P>
        where T : BTreeTemplateData
        where P : BTreeTemplateData
    {
        public BTreeEditorNode<T, P> m_EditorNode;
        public BTreeNodeDesigner<T, P> m_ParentNode;
        public List<BTreeNodeDesigner<T, P>> m_ChildNodeList;
        public List<BTreeNodeConnection<T, P>> m_ChildNodeConnectionList;
        public BTreeNodeConnection<T, P> m_ParentNodeConnection;
        public string m_NodeName = "";
        private bool m_Selected;
        private bool m_IsDirty = true;
        private bool m_IsEntryDisplay;
        public bool m_IsParent { get; private set; }
        private bool m_IsShowHoverBar;
        private Texture m_Icon;

        public void select()
        {
            m_Selected = true;
        }
        public void deselect()
        {
            m_Selected = false;
        }
        public void movePosition(Vector2 delta)
        {
            Vector2 vector = m_EditorNode.m_Pos;
            vector += delta;
            m_EditorNode.m_Pos = vector;
            if (m_ParentNode != null)
            {
                m_ParentNode.markDirty();
            }
            markDirty();
        }
        public void markDirty()
        {
            m_IsDirty = true;
        }
        public Rect IncomingConnectionRect(Vector2 offset)
        {
            Rect rect = this.rectangle(offset, false);
            return new Rect(rect.x + (rect.width - (float)BTreeEditorUtility.ConnectionWidth) / 2f, rect.y - (float)BTreeEditorUtility.TopConnectionHeight, (float)BTreeEditorUtility.ConnectionWidth, (float)BTreeEditorUtility.TopConnectionHeight);
        }
        public Rect OutgoingConnectionRect(Vector2 offset)
        {
            Rect rect = this.rectangle(offset, false);
            return new Rect(rect.x + (rect.width - (float)BTreeEditorUtility.ConnectionWidth) / 2f, rect.yMax, (float)BTreeEditorUtility.ConnectionWidth, (float)BTreeEditorUtility.BottomConnectionHeight);
        }
        public void init()
        {
            if (m_EditorNode == null)
            {
                Debugger.LogError("BTreeNodeDesigner init fail");
                return;
            }
            m_NodeName = m_EditorNode.m_Node.m_Name;
            m_IsParent = m_EditorNode.m_Node.m_ChildCount != 0;
            loadTaskIcon();
        }
        public void makeEntryDisplay(BTreeNodeDesigner<T, P> _child)
        {
            this.m_IsEntryDisplay = (this.m_IsParent = true);
            this.m_NodeName = "Entry";
            this.m_ChildNodeList = new List<BTreeNodeDesigner<T, P>>();
            m_ChildNodeList.Add(_child);
            m_ChildNodeConnectionList = new List<BTreeNodeConnection<T, P>>();
            m_ChildNodeConnectionList.Add(new BTreeNodeConnection<T, P>(_child,this,NodeConnectionType.Outgoing));
        }
        //绘制节点
        public bool drawNode(Vector2 offset, bool drawSelected, bool disabled)
        {
            Rect rect = this.rectangle(offset, false);
            GUI.color = Color.white;
            //上部
            if (!m_IsEntryDisplay)
            {
                GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.y - BTreeEditorUtility.TopConnectionHeight - BTreeEditorUtility.TaskBackgroundShadowSize + 4f, (float)BTreeEditorUtility.ConnectionWidth, (BTreeEditorUtility.TopConnectionHeight + BTreeEditorUtility.TaskBackgroundShadowSize)), BTreeEditorUtility.TaskConnectionTopTexture, ScaleMode.ScaleToFit);
                //GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.yMin - 3f, BTreeEditorUtility.ConnectionWidth, (BTreeEditorUtility.BottomConnectionHeight + BTreeEditorUtility.TaskBackgroundShadowSize)), BTreeEditorUtility.TaskConnectionTopTexture, ScaleMode.ScaleToFit);
            }
            //下部
            if (m_IsEntryDisplay || !m_EditorNode.m_Node.m_IsAcitonNode)
            {
                GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.yMax - 3f, BTreeEditorUtility.ConnectionWidth, (BTreeEditorUtility.BottomConnectionHeight + BTreeEditorUtility.TaskBackgroundShadowSize)), BTreeEditorUtility.TaskConnectionBottomTexture, ScaleMode.ScaleToFit);
            }
            //背景
            GUI.Label(rect, "", m_Selected ? BTreeEditorUtility.TaskSelectedGUIStyle : BTreeEditorUtility.TaskGUIStyle);
            //图标背景
            GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.IconBorderSize) / 2f, rect.y + ((BTreeEditorUtility.IconAreaHeight - BTreeEditorUtility.IconBorderSize) / 2) + 2f, BTreeEditorUtility.IconBorderSize, BTreeEditorUtility.IconBorderSize), BTreeEditorUtility.TaskBorderTexture);
            //图标
            GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.IconSize) / 2f, rect.y + ((BTreeEditorUtility.IconAreaHeight - BTreeEditorUtility.IconSize) / 2) + 2f, BTreeEditorUtility.IconSize, BTreeEditorUtility.IconSize), m_Icon);
            if (m_IsShowHoverBar)
            {
                GUI.DrawTexture(new Rect(rect.x - 1f, rect.y - 17f, 14f, 14f), m_EditorNode.m_Disable ? BTreeEditorUtility.EnableTaskTexture : BTreeEditorUtility.DisableTaskTexture, ScaleMode.ScaleToFit);
                if (m_IsParent)
                {
                    GUI.DrawTexture(new Rect(rect.x + 15f, rect.y - 17f, 14f, 14f), m_EditorNode.m_IsCollapsed ? BTreeEditorUtility.ExpandTaskTexture : BTreeEditorUtility.CollapseTaskTexture, ScaleMode.ScaleToFit);
                }
            }
            GUI.Label(new Rect(rect.x, rect.yMax - BTreeEditorUtility.TitleHeight - 1f, rect.width, BTreeEditorUtility.TitleHeight), this.ToString(), BTreeEditorUtility.TaskTitleGUIStyle);
            return true;
        }
        //绘制连线
        public void drawNodeConnection(Vector2 offset, float graphZoom, bool disabled)
        {
            if (m_IsDirty)
            {
                determineConnectionHorizontalHeight(rectangle(offset, false), offset);
                m_IsDirty = false;
            }
            if (m_IsParent)
            {
                for (int i = 0; i < this.m_ChildNodeConnectionList.Count; i++)
                {
                    m_ChildNodeConnectionList[i].drawConnection(offset, graphZoom, disabled);
                }
            }
        }
        //绘制节点说明
        public void drawNodeComment(Vector2 offset)
        {
            
        }
        //获取连线位置
        public Vector2 getConnectionPosition(Vector2 offset, NodeConnectionType connectionType)
        {
            Vector2 result;
            if (connectionType == NodeConnectionType.Incoming)
            {
                Rect rect = this.IncomingConnectionRect(offset);
                result = new Vector2(rect.center.x, rect.y + (float)(BTreeEditorUtility.TopConnectionHeight / 2));
            }
            else
            {
                Rect rect2 = this.OutgoingConnectionRect(offset);
                result = new Vector2(rect2.center.x, rect2.yMax - (float)(BTreeEditorUtility.BottomConnectionHeight / 2));
            }
            return result;
        }

        private void loadTaskIcon()
        {
            Texture2D _icon = null;
            if (m_IsEntryDisplay)
            {
                _icon = BTreeEditorUtility.LoadTexture("EntryIcon.png");
            }
            else if (m_EditorNode.m_Node.m_IsAcitonNode)
            {
                _icon = BTreeEditorUtility.LoadTexture("ActionIcon.png");
            }
            else
            {
                Type type = m_EditorNode.m_Node.GetType();
                if (type == typeof(BTreeNodePrioritySelector<T, P>))
                {
                    _icon = BTreeEditorUtility.PrioritySelectorIcon;
                }
                else if (type == typeof(BTreeNodeNonePrioritySelector<T, P>))
                {
                    _icon = BTreeEditorUtility.PrioritySelectorIcon;
                }
                else if (type == typeof(BTreeNodeSequence<T, P>))
                {
                    _icon = BTreeEditorUtility.SequenceIcon;

                }
                else if (type == typeof(BTreeNodeParallel<T, P>))
                {
                    _icon = BTreeEditorUtility.ParallelSelectorIcon;
                }
                else
                {
                    _icon = BTreeEditorUtility.InverterIcon;
                }
            }
            m_Icon = _icon;
        }
        
        private Rect rectangle(Vector2 offset, bool includeConnections)
        {
            Rect result = this.rectangle(offset);
            if (includeConnections)
            {
                if (!this.m_IsEntryDisplay)
                {
                    result.yMin = (result.yMin - (float)BTreeEditorUtility.TopConnectionHeight);
                }
                if (this.m_IsParent)
                {
                    result.yMax = (result.yMax + (float)BTreeEditorUtility.BottomConnectionHeight);
                }
            }
            return result;
        }
        private Rect rectangle(Vector2 offset)
        {
            if (this.m_EditorNode == null)
            {
                return default(Rect);
            }
            float num = BTreeEditorUtility.TaskTitleGUIStyle.CalcSize(new GUIContent(this.ToString())).x + (float)BTreeEditorUtility.TextPadding;
            if (!m_IsParent)
            {
                float num2;
                float num3;
                BTreeEditorUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent("Comment(Test)"), out num2, out num3);
                num3 += (float)BTreeEditorUtility.TextPadding;
                num = ((num > num3) ? num : num3);
            }
            num = Mathf.Min((float)BTreeEditorUtility.MaxWidth, Mathf.Max((float)BTreeEditorUtility.MinWidth, num));
            return new Rect(this.m_EditorNode.m_Pos.x + offset.x - num / 2f, this.m_EditorNode.m_Pos.y + offset.y, num, (float)(BTreeEditorUtility.IconAreaHeight + BTreeEditorUtility.TitleHeight));
        }
        //确定连线横向高度
        private void determineConnectionHorizontalHeight(Rect nodeRect, Vector2 offset)
        {
            if (this.m_IsParent)
            {
                float num = 3.40282347E+38f;
                float num2 = num;
                for (int i = 0; i < this.m_ChildNodeConnectionList.Count; i++)
                {
                    Rect rect = this.m_ChildNodeConnectionList[i].m_DestinationNodeDesigner.rectangle(offset, false);
                    if (rect.y < num)
                    {
                        num = rect.y;
                        num2 = rect.y;
                    }
                }
                num = num * 0.75f + nodeRect.yMax * 0.25f;
                if (num < nodeRect.yMax + 15f)
                {
                    num = nodeRect.yMax + 15f;
                }
                else if (num > num2 - 15f)
                {
                    num = num2 - 15f;
                }
                for (int j = 0; j < this.m_ChildNodeConnectionList.Count; j++)
                {
                    this.m_ChildNodeConnectionList[j].m_HorizontalHeight = num;
                }
            }
        }
        //是否包含坐标
        public bool contains(Vector2 point, Vector2 offset, bool includeConnections)
        {
            return rectangle(offset, includeConnections).Contains(point);
        }
       
        public override string ToString()
        {
            if (m_NodeName == null)
            {
                return "";
            }
            return m_NodeName;
        }
    }
}
