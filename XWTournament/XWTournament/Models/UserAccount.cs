using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace XWTournament.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; } = "";
        public bool IsAdmin { get; set; } = false;
        public int LoginFails { get; set; } = 0;
        public string APIPassword { get; set; }
    }

    public class MyPrincipal : IPrincipal
    {
        public MyPrincipal(IIdentity identity)
        {
            Identity = identity;
        }
        public IIdentity Identity
        {
            get;
            private set;
        }
        public UserAccount User { get; set; }
        public bool IsInRole(string role)
        {
            return true;
        }
    }
}
