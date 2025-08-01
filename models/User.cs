﻿using System;
using System.Collections.Generic;

namespace Task4_UserManagement.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Status { get; set; } = "active";
    public DateTime? LastLogin { get; set; }
    public DateTime RegistrationTime { get; set; }
}
