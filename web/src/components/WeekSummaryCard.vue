<template>
  <div class="week-card">
    <div class="week-header">
      <span class="week-label">Semana {{ summary.summary.weekNumber }}, {{ summary.summary.year }}</span>
      <TrendBadge :trend="summary.trend" />
    </div>
    <div class="week-body">
      <div class="week-stat">
        <span class="stat-label">Score</span>
        <span class="stat-value">{{ summary.summary.weeklyScore }}</span>
      </div>
      <div class="week-stat">
        <span class="stat-label">Comida</span>
        <span class="stat-value">{{ summary.summary.totalFood }}</span>
      </div>
      <div class="week-stat">
        <span class="stat-label">Ejercicio</span>
        <span class="stat-value">{{ summary.summary.totalExercise }}</span>
      </div>
      <div class="week-stat" v-if="summary.summary.weightKg">
        <span class="stat-label">Peso</span>
        <span class="stat-value">{{ summary.summary.weightKg }} kg</span>
      </div>
      <div class="week-dates">
        {{ summary.summary.weekStart }} → {{ summary.summary.weekEnd }}
      </div>
    </div>
  </div>
</template>

<script setup>
import TrendBadge from './TrendBadge.vue'

defineProps({
  summary: { type: Object, required: true },
})
</script>

<style scoped>
.week-card {
  background: #fff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.07);
}

.week-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

.week-label {
  font-weight: 600;
  font-size: 0.9rem;
}

.week-body {
  display: flex;
  gap: 1.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.week-stat {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 0.7rem;
  color: #888;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.stat-value {
  font-size: 1.1rem;
  font-weight: 700;
}

.week-dates {
  font-size: 0.75rem;
  color: #aaa;
  margin-left: auto;
}
</style>
