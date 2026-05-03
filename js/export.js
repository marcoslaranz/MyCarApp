// CSV download helper
window.downloadFile = function (fileName, mimeType, content) {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// Excel (.xlsx) download helper using SheetJS
window.downloadXlsx = function (fileName, rows) {
    const ws = XLSX.utils.aoa_to_sheet(rows);

    // Auto-width columns
    const colWidths = rows[0].map((_, colIndex) => ({
        wch: Math.max(...rows.map(row => (row[colIndex] ?? '').toString().length)) + 2
    }));
    ws['!cols'] = colWidths;

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Log Entries');
    XLSX.writeFile(wb, fileName);
};
