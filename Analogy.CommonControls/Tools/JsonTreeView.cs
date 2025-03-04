﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Analogy.CommonControls.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Analogy.CommonControls.Tools
{
    [Browsable(false)]
    [DesignTimeVisible(false)]
    [Designer("JSON Tree View")]
    public class JsonTreeView : TreeView
    {
        private TreeNode previouslySelectedNode;
        private ContextMenuStrip treeContextMenu;
        private IContainer components;
        private ToolStripMenuItem expandAllMenuItem;
        private StatusStrip statusStrip1;
        private string previouslySelectedNodeText;
        public event EventHandler<string> OnNodeChanged;
        public JsonTreeView()
        {
            InitializeComponent();
            AfterSelect += this_AfterSelect;
            DoubleClick += (s, e) =>
            {
                if (SelectedNode != null)
                {
                    Clipboard.SetText(SelectedNode.Text,TextDataFormat.UnicodeText);
                }
            };
            MouseDown += this_MouseDown;
            expandAllMenuItem.Click += expandAllMenuItem_Click;

            LoadImgaeList();
        }

        private void LoadImgaeList()
        {
            ImageList treeImages = new ImageList();
            treeImages.ImageSize = new Size(16, 16);
            foreach (JTokenType type in Enum.GetValues(typeof(JTokenType)))
            {
                Bitmap image;
                switch (type)
                {
                    case JTokenType.None:
                        image = Resources.None;
                        break;
                    case JTokenType.Object:
                        image = Resources.Object;
                        break;
                    case JTokenType.Array:
                        image = Resources.Array;
                        break;
                    case JTokenType.Constructor:
                        image = Resources.None;
                        break;
                    case JTokenType.Property:
                        image = Resources.None;
                        break;
                    case JTokenType.Comment:
                        image = Resources.None;
                        break;
                    case JTokenType.Integer:
                        image = Resources.Integer;
                        break;
                    case JTokenType.Float:
                        image = Resources.Float;
                        break;
                    case JTokenType.String:
                        image = Resources.String;
                        break;
                    case JTokenType.Boolean:
                        image = Resources.Boolean;
                        break;
                    case JTokenType.Null:
                        image = Resources.None;
                        break;
                    case JTokenType.Undefined:
                        image = Resources.Undefined;
                        break;
                    case JTokenType.Date:
                        image = Resources.Date;
                        break;
                    case JTokenType.Raw:
                        image = Resources.Object;
                        break;
                    case JTokenType.Bytes:
                        image = Resources.Object;
                        break;
                    case JTokenType.Guid:
                        image = Resources.Guid;
                        break;
                    case JTokenType.Uri:
                        image = Resources.Object;
                        break;
                    case JTokenType.TimeSpan:
                        image = Resources.TimeSpan;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                treeImages.Images.Add(type.ToString(), image ?? Resources.Undefined);
            }
            ImageList = treeImages;
        }

        public void ClearList()
        {
            Nodes.Clear();
        }
        public void ShowJson(string jsonString)
        {
            try
            {
                object json = JsonConvert.DeserializeObject(jsonString);
                LoadTree(json);
            }
            catch (Exception e)
            {
            }

        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private void LoadTree(object jsonData)
        {
            Nodes.Clear();
            if (jsonData is JArray jsonArray)
            {
                var rootNode = new JsonTreeNode(JsonNodeType.Object, "(root array)");
                foreach (JToken jToken in jsonArray)
                {
                    var node = new JsonTreeNode(JsonNodeType.Array, jToken.Type.ToString());
                    rootNode.Nodes.Add(node);
                    LoadObject(jToken as JObject, node);
                }
                TreeNode[] rootNodeArray = new TreeNode[rootNode.Nodes.Count];
                rootNode.Nodes.CopyTo(rootNodeArray, 0);
                Nodes.Add(rootNode);
                SelectedNode = rootNodeArray.First() as JsonTreeNode;
                rootNode.ImageKey = rootNode.NodeType.ToString();
                rootNode.SelectedImageKey = rootNode.ImageKey;
                rootNode.ExpandAll();
            }

            if (jsonData is JObject json)
            {
                var rootNode = new JsonTreeNode(JsonNodeType.Object, "(root)");
                Nodes.Add(rootNode);
                SelectedNode = rootNode;
                rootNode.ImageKey = rootNode.NodeType.ToString();
                rootNode.SelectedImageKey = rootNode.ImageKey;
                LoadObject(json, rootNode);
                rootNode.ExpandAll();
            }
        }

        private void AddNode(JsonTreeNode parentNode, string property, JToken item)
        {
            var node = JsonTreeNodeCreator.CreateNode(property, item);
            parentNode.Nodes.Add(node);

            if (item.Type == JTokenType.Array)
            {
                LoadArray(item, node);
            }

            if (item.Type == JTokenType.Object)
            {
                LoadObject(item as JObject, node);
            }
        }

        private void LoadArray(JToken value, JsonTreeNode node)
        {
            foreach (var item in value)
            {
                AddNode(node, null, item);
            }
        }

        private void LoadObject(JObject obj, JsonTreeNode node)
        {
            if (obj is null)
            {
                return;
            }

            foreach (var item in obj)
            {
                AddNode(node, item.Key, item.Value);
            }
        }

        private void InitializeComponent()
        {
            components = new Container();
            treeContextMenu = new ContextMenuStrip(components);
            expandAllMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            treeContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // treeContextMenu
            // 
            treeContextMenu.ImageScalingSize = new Size(20, 20);
            treeContextMenu.Items.AddRange(new ToolStripItem[] {
            expandAllMenuItem});
            treeContextMenu.Name = "treeContextMenu";
            treeContextMenu.Size = new Size(147, 30);
            treeContextMenu.Opening += new CancelEventHandler(treeContextMenu_Opening);
            // 
            // expandAllMenuItem
            // 
            expandAllMenuItem.Name = "expandAllMenuItem";
            expandAllMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.E)));
            expandAllMenuItem.ShowShortcutKeys = false;
            expandAllMenuItem.Size = new Size(146, 26);
            expandAllMenuItem.Text = "&Expand All";
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Location = new Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(200, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // JsonTreeView
            // 
            ContextMenuStrip = treeContextMenu;
            FullRowSelect = true;
            treeContextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        private new JsonTreeNode? SelectedNode
        {
            get => base.SelectedNode as JsonTreeNode;
            set => base.SelectedNode = value;
        }

        #region UI events

        private void this_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // restore previous seelcted node text and store the next
            if (previouslySelectedNode != null)
            {
                previouslySelectedNode.Text = previouslySelectedNodeText;
            }
            previouslySelectedNode = e.Node;
            previouslySelectedNodeText = e.Node.Text;

            e.Node.Text = ((JsonTreeNode)e.Node).TextWhenSelected;
            OnNodeChanged?.Invoke(this,e.Node.Text);
        }
        
        private void this_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = GetNodeAt(e.Location);
                if (node != null)
                {
                    base.SelectedNode = node;
                }
            }
        }

        private void treeContextMenu_Opening(object sender, CancelEventArgs e)
        {
            expandAllMenuItem.Visible = SelectedNode.IsExpandable;
            expandAllMenuItem.Enabled = SelectedNode.Nodes.Count > 0;
        }

        private void expandAllMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedNode != null)
            {
                BeginUpdate();
                SelectedNode.ExpandAll();
                EndUpdate();
            }
        }

        #endregion
    }
}
