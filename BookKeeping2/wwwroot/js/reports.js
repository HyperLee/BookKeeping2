(function () {
    const canvas = document.getElementById('monthlyTrendChart');
    if (!canvas || typeof Chart === 'undefined') {
        return;
    }

    const points = JSON.parse(canvas.dataset.points || '[]');
    const labels = points.map(point => point.Label);
    const income = points.map(point => point.Income);
    const expense = points.map(point => point.Expense);
    const incomeLabel = canvas.dataset.incomeLabel || '收入';
    const expenseLabel = canvas.dataset.expenseLabel || '支出';

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
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}());
