import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTreeModule } from '@angular/material/tree';
import { MatButtonModule } from '@angular/material/button';
import { NestedTreeControl } from '@angular/cdk/tree';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { PermissionDto } from './user.service';

interface PermissionTreeNode {
  name: string;
  type: 'group' | 'permission';
  key?: string;
  count?: number;
  children?: PermissionTreeNode[];
}

@Component({
  selector: 'permission-tree',
  standalone: true,
  imports: [CommonModule, MatTreeModule, MatButtonModule],
  templateUrl: './permission-tree.component.html',
  styleUrls: ['./permission-tree.component.scss'],
})
export class PermissionTreeComponent implements OnChanges {
  @Input() permissions: PermissionDto[] = [];

  treeControl = new NestedTreeControl<PermissionTreeNode>((node) => node.children);
  dataSource = new MatTreeNestedDataSource<PermissionTreeNode>();

  ngOnChanges(): void {
    this.dataSource.data = this.buildTree(this.permissions || []);
    this.treeControl.dataNodes = this.dataSource.data;
    // Expand all groups by default
    setTimeout(() => this.expandAll(), 0);
  }

  hasChild = (_: number, node: PermissionTreeNode) => !!node.children && node.children.length > 0;

  expandAll(): void {
    this.treeControl.dataNodes.forEach((node) => this.treeControl.expand(node));
  }

  collapseAll(): void {
    this.treeControl.collapseAll();
  }

  private buildTree(data: PermissionDto[]): PermissionTreeNode[] {
    const groupMap = new Map<string, PermissionTreeNode>();
    const root: PermissionTreeNode[] = [];
    const directPermissions: PermissionTreeNode[] = [];

    for (const p of data) {
      const groupName = (p.groupName || '').trim();

      if (!groupName) {
        directPermissions.push({
          name: p.permissionName,
          type: 'permission',
          key: p.permissionKey,
        });
        continue;
      }

      if (!groupMap.has(groupName)) {
        const groupNode: PermissionTreeNode = {
          name: groupName,
          type: 'group',
          count: 0,
          children: [],
        };
        groupMap.set(groupName, groupNode);
        root.push(groupNode);
      }

      groupMap.get(groupName)!.children!.push({
        name: p.permissionName,
        type: 'permission',
        key: p.permissionKey,
      });
    }

    if (directPermissions.length) {
      root.push({
        name: 'Direct Permissions',
        type: 'group',
        count: directPermissions.length,
        children: directPermissions,
      });
    }

    root.sort((a, b) => a.name.localeCompare(b.name));
    for (const node of root) {
      if (node.children) {
        node.count = node.children.length;
        node.children.sort((a, b) => {
          const left = a.key || a.name;
          const right = b.key || b.name;
          return left.localeCompare(right);
        });
      }
    }

    return root;
  }
}
