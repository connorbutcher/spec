using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Hubs;

public interface ICheckInOutHub
{
    /// <summary>
    /// Check in/out summary
    /// </summary>
    /// <param name="checkoutSummary">The check in out summary.</param>        
    Task CheckInOutSummary(CheckoutSummary checkoutSummary);

    /// <summary>
    /// Inform a client that they have had an item requested to be checked out by someone else.
    /// </summary>
    /// <param name="checkoutRequest">The request.</param>
    Task CheckoutRequested(CheckoutRequest checkoutRequest);

    /// <summary>
    /// Inform a client that their checkout has been overridden.
    /// </summary>
    /// <param name="checkoutRequest">The request.</param>
    Task OverrideCheckoutRequested(CheckoutRequest checkoutRequest);

    /// <summary>
    /// Informs a client that their checkout has been lost.
    /// </summary>
    /// <param name="checkoutRequest">The request.</param>        
    Task CheckoutLost(CheckoutRequest checkoutRequest);

    /// <summary>
    /// Inform a client that their request to check out an item has been accepted.
    /// </summary>
    /// <param name="checkoutRequest">The request.</param>
    Task CheckoutRequestAccepted(CheckoutRequest checkoutRequest);

    /// <summary>
    /// Inform a client that their request to check out an item has been rejected.
    /// </summary>
    /// <param name="checkoutRequest">The request.</param>
    Task CheckoutRequestRejected(CheckoutRequest checkoutRequest);

    /// <summary>
    /// Inform a client that the request to check out an item has been cancelled.
    /// </summary>
    /// <param name="checkoutRequestId">The context id.</param>
    Task CancelCheckoutRequest(Guid checkoutRequestId);

    ///// <summary>
    ///// Inform a client that a PU card has been saved.
    ///// </summary>
    ///// <param name="id">The id of the PU card.</param>
    ///// <param name="user">The user who has saved the PU card.</param>
    //Task PUCardSaved(int id, string user);

    ///// <summary>
    ///// Inform a client that a PU card has been locked.
    ///// </summary>
    ///// <param name="id">The id of the PU card.</param>
    ///// <param name="user">The user who has locked the pu card.</param>
    //Task PUCardLocked(int id, string user);
}
