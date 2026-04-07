<template>
  <div class="chart-container">
    <Line :data="chartData" :options="chartOptions" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

const props = defineProps({
  summaries: { type: Array, required: true },
})

const chartData = computed(() => {
  const now = new Date()
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
  const past = props.summaries.filter(s => s.summary.weekEnd < today)
  const reversed = [...past].reverse()
  const labels = reversed.map(s => `S${s.summary.weekNumber}`)
  const scores = reversed.map(s => s.summary.weeklyScore)
  const weights = reversed.map(s => s.summary.weightKg ?? null)
  const hasWeights = weights.some(w => w !== null)

  const datasets = [
    {
      label: 'Score',
      data: scores,
      borderColor: '#1a1a1a',
      backgroundColor: 'rgba(26,26,26,0.1)',
      tension: 0.3,
      yAxisID: 'y',
    },
  ]

  if (hasWeights) {
    datasets.push({
      label: 'Peso (kg)',
      data: weights,
      borderColor: '#1565c0',
      backgroundColor: 'rgba(21,101,192,0.1)',
      tension: 0.3,
      yAxisID: 'y1',
      spanGaps: true,
    })
  }

  return { labels, datasets }
})

const chartOptions = {
  responsive: true,
  interaction: { mode: 'index', intersect: false },
  plugins: {
    legend: { position: 'bottom' },
  },
  scales: {
    y: { type: 'linear', display: true, position: 'left', title: { display: true, text: 'Score' } },
    y1: { type: 'linear', display: true, position: 'right', title: { display: true, text: 'Peso (kg)' }, grid: { drawOnChartArea: false } },
  },
}
</script>

<style scoped>
.chart-container {
  background: #fff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.07);
}
</style>
