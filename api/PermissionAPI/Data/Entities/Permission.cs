using System;
using System.Collections.Generic;

namespace PermissionAPI.Data.Entities;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string PermissionKey { get; set; } = null!;

    public string PermissionName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
