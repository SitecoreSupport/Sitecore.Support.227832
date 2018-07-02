using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using System;
using System.Web;

namespace Sitecore.Pipelines.LoggedIn
{
  /// <summary>
  /// Processor for storing a ticket.
  /// </summary>
  public class Ticket : LoggedInProcessor
  {
    /// <summary>
    /// Returns the ticket manager. Used to create a ticket.
    /// </summary>
    protected BaseTicketManager TicketManager
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns default name of the ticket cookie.
    /// </summary>
    protected string TicketCookieName
    {
      get;
      private set;
    }

    /// <summary>
    /// Specifies the number of days before client "remember me" information expires.
    /// </summary>
    protected TimeSpan ClientPersistentLoginDuration
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sitecore.Pipelines.LoggedIn.Ticket" /> class. Obsolete.
    /// </summary>
    [Obsolete("Please use another constructor instead.")]
    public Ticket() : this(ServiceLocator.ServiceProvider.GetRequiredService<BaseTicketManager>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sitecore.Pipelines.LoggedIn.Ticket" /> class.
    /// </summary>
    /// <param name="manager">TicketManager</param>
    public Ticket(BaseTicketManager manager) : this(manager, "sitecore_userticket", Settings.Authentication.ClientPersistentLoginDuration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sitecore.Pipelines.LoggedIn.Ticket" /> class.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="ticketCookieName"></param>
    /// <param name="clientPersistentLoginDuration"></param>
    protected Ticket(BaseTicketManager manager, string ticketCookieName, TimeSpan clientPersistentLoginDuration)
    {
      Assert.ArgumentNotNull(manager, "manager");
      this.TicketManager = manager;
      this.TicketCookieName = ticketCookieName;
      this.ClientPersistentLoginDuration = clientPersistentLoginDuration;
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
    protected virtual void AppendTicketCookie(LoggedInArgs args, string ticketId, HttpContextBase context)
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
