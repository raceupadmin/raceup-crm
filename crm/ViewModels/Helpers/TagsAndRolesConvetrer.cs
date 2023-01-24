﻿using crm.Models.user;
using crm.ViewModels.dialogs;
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
        tagsListItem financierItem = new tagsListItem(Role.financier);
        tagsListItem commentItem = new tagsListItem(Role.comment);
        tagsListItem creativeItem = new tagsListItem(Role.creative);
        tagsListItem mediaItem = new tagsListItem(Role.media);
        tagsListItem teamleadItem = new tagsListItem(Role.teamlead);
        tagsListItem buyerItem = new tagsListItem(Role.buyer);
        tagsListItem linkItem = new tagsListItem(Role.link);
        tagsListItem farmItem = new tagsListItem(Role.farm);
        tagsListItem tagsItem = new tagsListItem(Role.creative);
        #endregion
        public List<Role> TagsToRoles(List<tagsListItem> tags)
        {
            List<Role> roles = new();

            bool isAdmin = tags.Any(t => t.Name.Equals(Role.admin));
            bool isTeamLead = tags.Any(t => t.Name.Equals(Role.teamlead));
            bool isBuyer = tags.Any(t => t.Name.Equals(Role.buyer));
            bool isComment = tags.Any(t => t.Name.Equals(Role.comment));
            bool isMedia = tags.Any(t => t.Name.Equals(Role.media));
            bool isLink = tags.Any(t => t.Name.Equals(Role.link));
            bool isFarm = tags.Any(t => t.Name.Equals(Role.farm));
            bool isCreative = tags.Any(t => t.Name.Equals(Role.creative));
            bool isFinancier = tags.Any(t => t.Name.Equals(Role.financier));

            if (isAdmin)
                roles.Add(new Role(RoleType.admin));

            if (isTeamLead)
            {
                if (isComment)
                    roles.Add(new Role(RoleType.team_lead_comment));
                if (isMedia)
                    roles.Add(new Role(RoleType.team_lead_media));
                if (isLink)
                    roles.Add(new Role(RoleType.team_lead_link));
                if (isFarm)
                    roles.Add(new Role(RoleType.team_lead_farm));
            } else
            {
                if (isComment)
                    roles.Add(new Role(RoleType.buyer_comment));
                if (isMedia)
                    roles.Add(new Role(RoleType.buyer_media));
                if (isLink)
                    roles.Add(new Role(RoleType.buyer_link));
                if (isFarm)
                    roles.Add(new Role(RoleType.buyer_farm));
            }

            if (isFinancier)
                roles.Add(new Role(RoleType.financier));
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

            bool fin = roles.Any(r => r.Type == RoleType.financier);
            if (fin)
                tags.Add(financierItem);

            bool cre = roles.Any(r => r.Type == RoleType.creative);
            if (cre)
                tags.Add(creativeItem);

            bool tl_comment = roles.Any(r => r.Type == RoleType.team_lead_comment);
            if (tl_comment)
            {
                tags.Add(teamleadItem);
                tags.Add(commentItem);
            }
            bool buyer_comment = roles.Any(r => r.Type == RoleType.buyer_comment);
            if (buyer_comment)
            {
                tags.Add(buyerItem);
                tags.Add(commentItem);
            }

            bool tl_farm = roles.Any(r => r.Type == RoleType.team_lead_farm);
            if (tl_comment)
            {
                tags.Add(teamleadItem);
                tags.Add(farmItem);
            }
            bool buyer_farm = roles.Any(r => r.Type == RoleType.buyer_farm);
            if (buyer_farm)
            {
                tags.Add(buyerItem);
                tags.Add(farmItem);
            }

            bool tl_link = roles.Any(r => r.Type == RoleType.team_lead_link);
            if (tl_link)
            {
                tags.Add(teamleadItem);
                tags.Add(linkItem);
            }
            bool buyer_link = roles.Any(r => r.Type == RoleType.buyer_link);
            if (buyer_link)
            {
                tags.Add(buyerItem);
                tags.Add(linkItem);
            }

            bool tl_media = roles.Any(r => r.Type == RoleType.team_lead_media);
            if (tl_media)
            {
                tags.Add(teamleadItem);
                tags.Add(mediaItem);
            }
            bool buyer_media = roles.Any(r => r.Type == RoleType.buyer_media);
            if (buyer_media)
            {
                tags.Add(buyerItem);
                tags.Add(mediaItem);
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

        public ObservableCollection<tagsListItem> GetAllTags()
        {
            ObservableCollection<tagsListItem> tags = new()
            {
                adminItem,
                financierItem,
                commentItem,
                buyerItem,
                mediaItem,
                teamleadItem,
                linkItem,
                farmItem,
                creativeItem
            };

            return tags;
        }

        public List<tagsListItem> GetAllTagsList()
        {
            List<tagsListItem> tags = new()
            {
                adminItem,
                financierItem,
                commentItem,
                buyerItem,
                mediaItem,
                teamleadItem,
                linkItem,
                farmItem,
                creativeItem
            };

            return tags;
        }
    }
}
