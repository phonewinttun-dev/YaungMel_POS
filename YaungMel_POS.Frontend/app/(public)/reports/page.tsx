"use client";

import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Card, CardHeader } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { SkeletonCard } from "@/components/ui/Skeleton";
import { toast } from "@/components/ui/Toast";
import { Pagination } from "@/components/ui/Pagination";
import { reportsApi, summariesApi } from "@/lib/api";
import type { SummaryDTO, PageSettingDTO } from "@/lib/types";
import {
  Calendar,
  ChevronLeft,
  ChevronRight,
  Download,
  FileText,
  Printer,
  Search,
  TrendingUp,
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";

export default function ReportsPage() {
  // State for summary table
  const [summaries, setSummaries] = useState<SummaryDTO[]>([]);
  const [pageSetting, setPageSetting] = useState<PageSettingDTO>({
    pageNo: 1,
    pageSize: 10,
    pageCount: 0,
  });
  const [isLoading, setIsLoading] = useState(true);

  // State for PDF generation
  const [reportDate, setReportDate] = useState(new Date().toISOString().split("T")[0]);
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [isGeneratingDaily, setIsGeneratingDaily] = useState(false);
  const [isGeneratingRange, setIsGeneratingRange] = useState(false);

  const loadSummaries = useCallback(async (page: number) => {
    setIsLoading(true);
    try {
      const res = await summariesApi.getPaged(page, pageSetting.pageSize);
      if (res.isSuccess && res.data) {
        setSummaries(res.data.items);
        setPageSetting(res.data.pageSetting);
      }
    } catch {
      toast("error", "Failed to load summary data");
    } finally {
      setIsLoading(false);
    }
  }, [pageSetting.pageSize]);

  useEffect(() => {
    void loadSummaries(1);
  }, [loadSummaries]);

  const handleDownloadPdf = async (blob: Blob, fileName: string) => {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", fileName);
    document.body.appendChild(link);
    link.click();
    link.parentNode?.removeChild(link);
    window.URL.revokeObjectURL(url);
  };

  const generateDailyReport = async () => {
    if (!reportDate) {
      toast("error", "Please select a date");
      return;
    }
    setIsGeneratingDaily(true);
    try {
      const blob = await reportsApi.generateDaily(reportDate);
      await handleDownloadPdf(blob, `Daily_Report_${reportDate}.pdf`);
      toast("success", "Daily report generated successfully");
    } catch {
      toast("error", "Failed to generate daily report");
    } finally {
      setIsGeneratingDaily(false);
    }
  };

  const generateRangeReport = async () => {
    if (!startDate || !endDate) {
      toast("error", "Please select start and end dates");
      return;
    }
    setIsGeneratingRange(true);
    try {
      const blob = await reportsApi.generateRange(startDate, endDate);
      await handleDownloadPdf(blob, `Range_Report_${startDate}_to_${endDate}.pdf`);
      toast("success", "Range report generated successfully");
    } catch {
      toast("error", "Failed to generate range report");
    } finally {
      setIsGeneratingRange(false);
    }
  };

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">
              Sales Reports
            </h2>
            <p className="text-sm text-[var(--text-secondary)] mt-1">
              View and export detailed performance summaries
            </p>
          </div>
        </div>

        {/* Action Cards */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Daily Report Card */}
          <Card padding="lg" hover className="flex flex-col h-full">
            <CardHeader
              title="Daily Report"
              subtitle="Generate a detailed report for a specific day"
              icon={<Calendar size={20} className="text-[var(--accent-primary)]" />}
            />
            <div className="mt-4 flex-1 space-y-4">
              <Input
                label="Select Date"
                type="date"
                value={reportDate}
                onChange={(e) => setReportDate(e.target.value)}
                icon={<Calendar size={18} />}
              />
              <div className="flex gap-3 mt-6">
                <Button
                  className="flex-1"
                  onClick={generateDailyReport}
                  isLoading={isGeneratingDaily}
                  icon={<Download size={18} />}
                >
                  Download PDF
                </Button>
                <Button
                  variant="secondary"
                  onClick={() => toast("info", "Print preview coming soon")}
                  icon={<Printer size={18} />}
                >
                  Print
                </Button>
              </div>
            </div>
          </Card>

          {/* Range Report Card */}
          <Card padding="lg" hover className="flex flex-col h-full">
            <CardHeader
              title="Custom Range Report"
              subtitle="Detailed analysis for a selected period"
              icon={<TrendingUp size={20} className="text-emerald-500" />}
            />
            <div className="mt-4 flex-1 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="Start Date"
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                />
                <Input
                  label="End Date"
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </div>
              <div className="flex gap-3 mt-6">
                <Button
                  className="flex-1"
                  variant="primary"
                  onClick={generateRangeReport}
                  isLoading={isGeneratingRange}
                  icon={<Download size={18} />}
                >
                  Download PDF
                </Button>
              </div>
            </div>
          </Card>
        </div>

        {/* Summary Table */}
        <Card padding="lg">
          <CardHeader
            title="Summary History"
            subtitle="Recent daily sales performance"
            action={
              <Button
                variant="secondary"
                size="sm"
                onClick={() => void loadSummaries(1)}
                icon={<Search size={14} />}
              >
                Refresh
              </Button>
            }
          />
          <div className="overflow-x-auto mt-4">
            <table className="w-full">
              <thead>
                <tr className="border-b border-[var(--border-primary)]">
                  {["Date", "Total Sales", "Revenue", "Top Product", "Status"].map((h) => (
                    <th
                      key={h}
                      className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider"
                    >
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-primary)]">
                {isLoading ? (
                  Array.from({ length: 5 }).map((_, i) => (
                    <tr key={i}>
                      <td colSpan={5} className="py-4 px-4">
                        <div className="h-4 bg-[var(--bg-tertiary)] rounded animate-pulse w-full" />
                      </td>
                    </tr>
                  ))
                ) : summaries.length > 0 ? (
                  summaries.map((summary) => (
                    <tr
                      key={summary.date}
                      className="hover:bg-[var(--bg-hover)] transition-colors group"
                    >
                      <td className="py-3 px-4">
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-lg bg-[var(--bg-tertiary)] flex items-center justify-center text-[var(--accent-primary)]">
                            <FileText size={16} />
                          </div>
                          <span className="text-sm font-medium text-[var(--text-primary)]">
                            {new Date(summary.date).toLocaleDateString()}
                          </span>
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-sm text-[var(--text-primary)] font-mono">
                          {summary.totalSale}
                        </span>
                      </td>
                      <td className="py-3 px-4 font-mono text-sm text-[var(--text-primary)]">
                        {summary.totalAmountFormatted}
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-sm text-[var(--text-secondary)]">
                          {summary.topSaleProductName || "N/A"}
                        </span>
                      </td>
                      <td className="py-3 px-4">
                        <Badge variant="success">Completed</Badge>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={5} className="py-12 text-center text-[var(--text-tertiary)]">
                      No summary data found
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {pageSetting.pageCount > 1 && (
            <div className="mt-6 pt-6 border-t border-[var(--border-primary)]">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <p className="text-xs text-[var(--text-tertiary)]">
                  Page {pageSetting.pageNo} of {pageSetting.pageCount}
                </p>
                <Pagination
                  currentPage={pageSetting.pageNo}
                  totalPages={pageSetting.pageCount}
                  onPageChange={loadSummaries}
                />
              </div>
            </div>
          )}
        </Card>
      </div>
    </AnimatedPage>
  );
}
