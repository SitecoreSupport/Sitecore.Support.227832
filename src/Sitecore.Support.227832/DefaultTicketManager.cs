using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Properties;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.LoggedIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Support.Web.Authentication
{
  public class DefaultTicketManager : Sitecore.Web.Authentication.DefaultTicketManager
  {


    public DefaultTicketManager(BaseClient client) : base(client)
    {
    }
    public override string CreateTicket(string userName, string startUrl)
    {
      Assert.ArgumentNotNull(userName, "userName");
      return this.CreateTicket(userName, startUrl, true);
    }
    /// <summary>
    /// Creates the ticket.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="startUrl">The start URL.</param>
    /// <param name="persist">If set to <c>true</c> [persist].</param>
    /// <returns>
    /// The ticket.
    /// </returns>
    public override string CreateTicket(string userName, string startUrl, bool persist)
    {
      Assert.ArgumentNotNullOrEmpty(userName, "userName");
      Sitecore.Web.Authentication.Ticket ticket = GetExistingTicket(userName, true);
      if (ticket == null)
      {
        Sitecore.Web.Authentication.Ticket existingTicket = GetExistingTicket(userName, false);
        ticket = new Sitecore.Web.Authentication.Ticket
        {
          Id = ID.NewID.ToShortID().ToString(), //old value:((existingTicket != null) ? existingTicket.Id : null) ?? string.Empty) + ID.NewID.ToShortID().ToString()
          UserName = userName,
          Persist = persist
        };
      }
      ticket.TimeStamp = DateTime.UtcNow;
      if (!string.IsNullOrEmpty(startUrl))
      {
        this.CheckOnExternalUrl(startUrl);
        ticket.StartUrl = startUrl;
      }
      ticket.Persist = persist;
      if (!Settings.GetBoolSetting("Security.ShareAuthenticationTicket", false) && !string.IsNullOrEmpty(this.GetSessionId()))
      {
        ticket.ClientId = this.GetSessionId();
      }
      this.SaveTicket(ticket);
      return ticket.Id;
    }

    /// <summary>
    /// Gets the existing ticket.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="checkShareAuthentication">checkShareAuthentication.</param>
    /// <returns>
    /// The existing ticket.
    /// </returns>
    private Sitecore.Web.Authentication.Ticket GetExistingTicket(string userName, bool checkShareAuthentication = true)
    {
      Assert.ArgumentNotNullOrEmpty(userName, "userName");
      List<string> ticketKeyList = this.GetTicketKeyList();
      string sessionId = this.GetSessionId();
      if ((!Settings.GetBoolSetting("Security.ShareAuthenticationTicket", false) & checkShareAuthentication) && !string.IsNullOrEmpty(sessionId))
      {
        foreach (string current in ticketKeyList)
        {
          Sitecore.Web.Authentication.Ticket ticketByKey = base.GetTicketByKey(current);
          if (ticketByKey != null && ticketByKey.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && ticketByKey.ClientId.Equals(sessionId, StringComparison.OrdinalIgnoreCase))
          {
            Sitecore.Web.Authentication.Ticket result = (!this.IsTicketExpired(ticketByKey, true)) ? ticketByKey : null;
            return result;
          }
        }
        return null;
      }
      foreach (string current2 in ticketKeyList)
      {
        Sitecore.Web.Authentication.Ticket ticketByKey2 = base.GetTicketByKey(current2);
        if (ticketByKey2 != null && ticketByKey2.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
        {
          Sitecore.Web.Authentication.Ticket result = (!base.IsTicketExpired(ticketByKey2, true)) ? ticketByKey2 : null;
          return result;
        }
      }
      return null;
    }
  }
}
