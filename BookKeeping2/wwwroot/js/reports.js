(function () {
    const canvases = document.querySelectorAll('.monthly-trend-chart');
    if (canvases.length === 0 || typeof Chart === 'undefined') {
        return;
    }

    canvases.forEach((canvas) => {
        const points = JSON.parse(canvas.dataset.points || '[]');
        const labels = points.map(point => point.Label);
        const income = points.map(point => point.Income);
        const expense = points.map(point => point.Expense);
        const currency = canvas.dataset.currency || (points[0] && points[0].Currency) || '';
        const incomeLabel = `${currency} ${canvas.dataset.incomeLabel || '收入'}`.trim();
        const expenseLabel = `${currency} ${canvas.dataset.expenseLabel || '支出'}`.trim();

        new Chart(canvas, {
            type: 'line',
            data: {
                labels,
                datasets: [
                    {
                        label: incomeLabel,
                        data: income,
                        borderColor: '#198754',
                        backgroundColor: 'rgba(25, 135, 84, 0.12)',
                        tension: 0.2
                    },
                    {
                        label: expenseLabel,
                        data: expense,
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.12)',
                        tension: 0.2
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    tooltip: {
                        callbacks: {
                            label(context) {
                                return `${context.dataset.label}: ${context.parsed.y}`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    });
}());
