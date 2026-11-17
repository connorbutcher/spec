using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Services.SpecSheets;

/// <summary>
/// Defines a service for checking in and out PU cards.
/// </summary>
public interface IPUSpecSheetCheckInOutService
{
    #region Check Out

    /// <summary>
    /// Checks out a pu spec sheet.
    /// </summary>
    /// <param name="puSpecSheetId">The id of the pu spec sheet to checkout.</param>
    /// <param name="connectedUser">The user checking out the template definition.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> TryCheckoutPUSpecSheetAsync(int puSpecSheetId, ConnectedUser connectedUser);

    #endregion Check Out

    #region Check In

    /// <summary>
    /// Checks in a pu spec sheet.
    /// </summary>
    /// <param name="puSpecSheetId">The id of the pu spec sheet to checkin.</param>
    /// <param name="connectionId">The id of the user checking in the template definition.</param>
    /// <returns>True if the successful, false otherwise.</returns>
    Task<bool> TryCheckInPUSpecSheetAsync(int puSpecSheetId, string connectionId);

    /// <summary>
    /// Remove all check ins for the user.
    /// </summary>
    /// <param name="connectionId">The id of the user to remove the check ins for.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> TryRemoveAllCheckInsForUserAsync(string connectionId);

    #endregion Check In

    #region Request Checkouts

    /// <summary>
    /// Request checkout.
    /// </summary>
    /// <param name="id">The id of the item to request.</param>
    /// <param name="requestingUser">The requesting user.</param>
    /// <returns>The request checkout if successful.</returns>
    Task<CheckoutRequest?> RequestCheckoutAsync(int id, ConnectedUser requestingUser);

    /// <summary>
    /// Remove a request checkout.
    /// </summary>
    /// <param name="checkoutRequestContextId">The id of the request to remove.</param>
    /// <returns>The checkout request removed or null if not item removed.</returns>
    Task<CheckoutRequest?> TryRemoveCheckoutRequestAsync(Guid checkoutRequestContextId);

    /// <summary>
    /// Checks whether an item has had a checkout requested.
    /// </summary>
    /// <param name="id">The id of the item to check.</param>
    /// <returns>True or false.</returns>
    Task<bool> HasCheckoutBeenRequestedAsync(int id);

    #endregion Request Checkouts

    #region Gets

    Task<IReadOnlyList<PUSpecSheetCheckout>> GetAllPUSpecSheetCheckoutsAsync();

    /// <summary>
    /// Get a checkout request by id.
    /// </summary>
    /// <param name="requestContextId">The id to get.</param>
    /// <returns>The checkout request if present.</returns>
    Task<CheckoutRequest?> GetCheckoutRequestByIdAsync(Guid requestContextId);

    /// <summary>
    /// Get all the checkout requests for user.
    /// </summary>
    /// <param name="connectionId">The connection id of the user.</param>
    /// <returns>The checkout requests.</returns>
    Task<IReadOnlyList<CheckoutRequest>> GetAllCheckoutRequestsForUserAsync(string connectionId);

    #endregion Gets
}
