using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Helpers;

public static class ApprovalMessageHelper
{
    public static (int RequisitionCount, int POCount) GetSlipPendingCounts(
        IEnumerable<Request_RS_SlipVM> requisitionSlips,
        IEnumerable<Request_PO_SlipVM> poSlips)
    {
        var reqCount = requisitionSlips?
            .Count(e => e.Approval == ApprovalAction.Pending) ?? 0;

        var poCount = poSlips?
            .Count(e => e.Approval == ApprovalAction.Pending) ?? 0;

        return (reqCount, poCount);
    }

    public static string GetCEOPendingApprovalMessage(int requisitionCount, int poCount)
    {
        if (requisitionCount == 0 && poCount == 0)
            return "No pending approvals";

        var messages = new List<string>();

        if (requisitionCount > 0)
        {
            messages.Add($"{requisitionCount} requisition {Pluralize("slip", requisitionCount)}");
        }

        if (poCount > 0)
        {
            messages.Add($"{poCount} purchase order {Pluralize("slip", poCount)}");
        }

        if (messages.Count == 0) return string.Empty;

        if (messages.Count == 1)
        {
            return $"You have {messages[0]} awaiting your approval.";
        }

        // For 2 items: "You have X requisition slips and Y purchase order slips awaiting your approval."
        return $"You have {string.Join(" and ", messages)} awaiting your approval.";
    }

    public static string GetAdminPendingMessage(bool pendingRequisition, bool pendingPO)
    {
        if (pendingRequisition && pendingPO)
        {
            return "All requisition and purchase order slips must be approved before closing.";
        }
        else if (pendingRequisition)
        {
            return "All requisition slips must be approved before closing.";
        }
        else if (pendingPO)
        {
            return "All purchase order slips must be approved before closing.";
        }

        return string.Empty;
    }

    public static (bool hasDiscrepancies, string message) GetAdminAmountDiscrepancyMessage(IRequestDetailVM request)
    {
        if (request == null)
            return (false, "Unable to verify amounts.");

        var discrepancies = new List<string>();

        // 1. Check Requisition Slips vs Receipts
        var requisitionDiscrepancies = CheckRequisitionAmounts(request);
        discrepancies.AddRange(requisitionDiscrepancies);

        // 2. Check PO Slips vs Receipts
        var poDiscrepancies = CheckPOAmounts(request);
        discrepancies.AddRange(poDiscrepancies);

        if (discrepancies.Count == 0)
        {
            return (false, "All receipt amounts match.");
        }

        return (true, $"Amount mismatch: {FormatDiscrepancies(discrepancies)}");
    }

    private static List<string> CheckRequisitionAmounts(IRequestDetailVM request)
    {
        var discrepancies = new List<string>();

        var requisitionSlips = request.RequisitionSlips?.Where(e => e.Approval == ApprovalAction.Approve).ToList() ?? [];
        var receiptAmounts = request.Attachments?
            .Where(a => a.AttachType == RequestAttachType.Receipt)
            .GroupBy(a => a.RequisitionId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.ReceiptAmount)) ?? []; ;

        foreach (var requisition in requisitionSlips)
        {
            var receiptTotal = receiptAmounts.GetValueOrDefault(requisition.Id, 0m);
            if (Math.Abs(requisition.AmountRequested - receiptTotal) > 0.01m)
            {
                discrepancies.Add($"Req #{requisition.SeriesNumber}");
            }
        }

        return discrepancies;
    }

    private static List<string> CheckPOAmounts(IRequestDetailVM request)
    {
        var discrepancies = new List<string>();

        var poSlips = request.POSlips?.Where(e => e.Approval == ApprovalAction.Approve).ToList() ?? [];

        foreach (var po in poSlips)
        {
            var poAmount = po.Total_Amount;
            var poNumber = po.SeriesNumber;
            // Get all receipts attached to this PO
            var poReceipts = request.Attachments?
                .Where(a => a.AttachType == RequestAttachType.Receipt && a.POId == po.Id)
                .ToList() ?? [];

            var totalReceipts = poReceipts.Sum(r => r.ReceiptAmount);

            // Compare PO total amount with sum of receipts
            if (Math.Abs(po.Total_Amount - totalReceipts) > 0.01m)
            {
                discrepancies.Add($"PO #{po.SeriesNumber}");
            }
        }

        return discrepancies;
    }

    private static string FormatDiscrepancies(List<string> discrepancies)
    {
        if (discrepancies.Count == 1) return discrepancies[0];
        if (discrepancies.Count == 2) return $"{discrepancies[0]} and {discrepancies[1]}";

        // For 3 or more: "X, Y, and Z"
        var allButLast = string.Join(", ", discrepancies.Take(discrepancies.Count - 1));
        return $"{allButLast}, and {discrepancies.Last()}";
    }

    private static string Pluralize(string word, int count)
    {
        return count == 1 ? word : word + "s";
    }
}