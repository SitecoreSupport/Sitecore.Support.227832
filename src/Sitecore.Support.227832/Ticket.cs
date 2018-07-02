using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.LoggedIn;
using System;
using System.Web;

namespace Sitecore.Support.Pipelines.LoggedIn
{
  /// <summary>
  /// Processor for storing a ticket.
  /// </summary>
  public class Ticket : Sitecore.Pipelines.LoggedIn.Ticket
  {

    public Ticket() : this(new Sitecore.Support.Web.Authentication.DefaultTicketManager(new Sitecore.DefaultClient()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sitecore.Pipelines.LoggedIn.Ticket" /> class.
    /// </summary>
    /// <param name="manager">TicketManager</param>
    public Ticket(Sitecore.Support.Web.Authentication.DefaultTicketManager manager) : base(manager, "sitecore_userticket", Settings.Authentication.ClientPersistentLoginDuration)
    {
    }

    /// <summary>
    /// Runs the processor.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public override void Process(LoggedInArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.ArgumentNotNullOrEmpty(args.Username, "args.Username");
      Assert.ArgumentNotNull(args.StartUrl, "args.StartUrl");
      string text = this.TicketManager.CreateTicket(args.Username, args.StartUrl, args.Persist);
      if (string.IsNullOrEmpty(text) || args.Context == null)
      {
        return;
      }
      this.AppendTicketCookie(args, text, args.Context);
    }

    /// <summary>
    /// Appends the ticket cookie.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="ticketId">The ticket identifier.</param>
    /// <param name="context">The context.</param>
    protected override void AppendTicketCookie(LoggedInArgs args, string ticketId, HttpContextBase context)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.ArgumentNotNull(context, "context");
      HttpCookie cookie = new HttpCookie(this.TicketCookieName, ticketId)
      {
        HttpOnly = true,
        Expires = (args.Persist ? DateTime.UtcNow.Add(this.ClientPersistentLoginDuration) : DateTime.MinValue)
      };
      context.Response.AppendCookie(cookie);
    }
  }
}
