// <copyright file="Tree.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Workspace.Meta;

    public class Tree
    {
        public Tree(IComposite composite)
        {
            this.Composite = composite;
            this.Nodes = new TreeNodes(this.Composite);
        }

        public IComposite Composite { get; }

        public TreeNodes Nodes { get; }

        public string DebugView
        {
            get
            {
                var toString = new StringBuilder();
                toString.Append(this.Composite.Name + "\n");
                this.DebugNodeView(toString, this.Nodes, 1);
                return toString.ToString();
            }
        }

        public Tree Add(IEnumerable<IRelationType> relationTypes) => this.Add(relationTypes.Select(v => v.RoleType));

        public Tree Add(IEnumerable<IRoleType> roleTypes)
        {
            new List<IRoleType>(roleTypes).ForEach(v => this.Add(v));
            return this;
        }

        public Tree Add(IRelationType relationType) => this.Add(relationType.RoleType);

        public Tree Add(IRoleType roleType)
        {
            var treeNode = new TreeNode(roleType);
            this.Nodes.Add(treeNode);
            return this;
        }

        public Tree Add(IConcreteRoleType concreteRoleType) => this.Add(concreteRoleType.RoleType);

        public Tree Add(IRelationType relationType, Tree tree) => this.Add(relationType.RoleType, tree);

        public Tree Add(IRoleType roleType, Tree tree)
        {
            var treeNode = new TreeNode(roleType, tree.Composite, tree.Nodes);
            this.Nodes.Add(treeNode);
            return this;
        }

        public Tree Add(IConcreteRoleType concreteRoleType, Tree tree)
        {
            var treeNode = new TreeNode(concreteRoleType.RoleType, tree.Composite, tree.Nodes);
            this.Nodes.Add(treeNode);
            return this;
        }

        public Protocol.Data.Tree ToJson() =>
            new Protocol.Data.Tree
            {
                Composite = this.Composite.Id,
                Nodes = this.Nodes.Select(v => v.ToJson()).ToArray(),
            };

        private void DebugNodeView(StringBuilder toString, TreeNodes nodes, int level)
        {
            foreach (var node in nodes)
            {
                var indent = new string(' ', level * 2);
                toString.Append(indent + "- " + node.RoleType + "\n");
                this.DebugNodeView(toString, node.Nodes, level + 1);
            }
        }
    }
}