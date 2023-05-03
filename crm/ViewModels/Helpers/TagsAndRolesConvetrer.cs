using crm.Models.user;
using crm.ViewModels.dialogs;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.ViewModels.Helpers
{
    public class TagsAndRolesConvetrer
    {
        #region vars
        tagsListItem adminItem = new tagsListItem(Role.admin);
        tagsListItem creativeItem = new tagsListItem(Role.creative);
        tagsListItem buyerItem = new tagsListItem(Role.buyer);
        tagsListItem linkItem = new tagsListItem(Role.link);
        tagsListItem superadminItem = new tagsListItem(Role.superadmin);
        tagsListItem developerItem = new tagsListItem(Role.developer);
        tagsListItem closerItem = new tagsListItem(Role.сloser);
        #endregion
        public List<Role> TagsToRoles(List<tagsListItem> tags)
        {
            List<Role> roles = new();

            bool isAdmin = tags.Any(t => t.Name.Equals(Role.admin));
            bool isBuyer = tags.Any(t => t.Name.Equals(Role.buyer));
            bool isLink = tags.Any(t => t.Name.Equals(Role.link));
            bool isCreative = tags.Any(t => t.Name.Equals(Role.creative));
            bool isDeveloper = tags.Any(t => t.Name.Equals(Role.developer));
            bool isCloser = tags.Any(t => t.Name.Equals(Role.сloser));
            bool isSuperAdmin = tags.Any(t => t.Name.Equals(Role.superadmin));

            if (isAdmin)
                roles.Add(new Role(RoleType.admin));

            if (isBuyer)
                roles.Add(new Role(RoleType.buyer));

            if (isLink)
                roles.Add(new Role(RoleType.link));

            if (isCloser) roles.Add(new Role(RoleType.сloser));

            if (isSuperAdmin)
                roles.Add(new Role(RoleType.superadmin));

            if (isDeveloper) roles.Add(new Role(RoleType.developer));
            
            if (isCreative)
                roles.Add(new Role(RoleType.creative));

            return roles;
        }
        public List<tagsListItem> RolesToTags(List<Role> roles)
        {
            List<tagsListItem> tags = new();

            bool adm = roles.Any(r => r.Type == RoleType.admin);
            if (adm)
                tags.Add(adminItem);

            bool sadm = roles.Any(r => r.Type == RoleType.superadmin);
            if (sadm) 
                tags.Add(superadminItem);

            bool buyer = roles.Any(r => r.Type == RoleType.buyer);
            if (buyer)
                tags.Add(buyerItem);

            bool cre = roles.Any(r => r.Type == RoleType.creative);
            if (cre)
                tags.Add(creativeItem);

            bool link = roles.Any(r => r.Type == RoleType.link);
            if (link)
            {
                tags.Add(linkItem);
            }
            bool clsr = roles.Any(r => r.Type == RoleType.сloser);
            if (clsr)
            {
                tags.Add(closerItem);
            }

            bool developer = roles.Any(r => r.Type == RoleType.developer);
            if (developer)
            {
                tags.Add(developerItem);
            }
            

            //bool tl = roles.Any(r =>
            //    r.Type == RoleType.team_lead_comment ||
            //    r.Type == RoleType.team_lead_farm ||
            //    r.Type == RoleType.team_lead_link ||
            //    r.Type == RoleType.team_lead_media
            //);
            //if (tl)
            //    tags.Add(teamleadItem);

            //bool fin = roles.Any(r => r.Type == RoleType.financier);
            //if (fin)
            //    tags.Add(financierItem);

            //bool com = roles.Any(r =>
            //   r.Type == RoleType.team_lead_comment ||
            //   r.Type == RoleType.buyer_comment
            //);
            //if (com)
            //    tags.Add(commentItem);

            //bool frm = roles.Any(r =>
            //   r.Type == RoleType.team_lead_farm ||
            //   r.Type == RoleType.buyer_farm
            //);
            //if (frm)
            //    tags.Add(farmItem);

            //bool lnk = roles.Any(r =>
            //   r.Type == RoleType.team_lead_link ||
            //   r.Type == RoleType.buyer_link
            //);
            //if (lnk)
            //    tags.Add(linkItem);

            //bool med = roles.Any(r =>
            //   r.Type == RoleType.team_lead_media ||
            //   r.Type == RoleType.buyer_media
            //);
            //if (med)
            //    tags.Add(mediaItem);

            //bool cre = roles.Any(r =>
            //   r.Type == RoleType.creative
            //);
            //if (cre)
            //    tags.Add(creativeItem);

            return tags;
        }

        public ObservableCollection<tagsListItem> GetAllTags(bool add_sa = true)
        {
            if (add_sa)
            {
                ObservableCollection<tagsListItem> tags = new()
                {
                superadminItem,
                adminItem,
                developerItem,
                buyerItem,
                linkItem,
                closerItem,
                creativeItem
                };

                return tags;
            }
            else
            {
                ObservableCollection<tagsListItem> tags = new()
                {
                adminItem,
                developerItem,
                buyerItem,
                linkItem,
                closerItem,
                creativeItem
                };

                return tags;
            }
        }

        public List<tagsListItem> GetAllTagsList()
        {
            List<tagsListItem> tags = new()
            {
                superadminItem,
                adminItem,
                developerItem,
                buyerItem,
                linkItem,
                closerItem,
                creativeItem
            };

            return tags;
        }
    }
}
