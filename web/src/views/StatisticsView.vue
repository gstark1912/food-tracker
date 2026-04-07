<template>
  <div class="stats-view">
    <header class="app-header">
      <RouterLink to="/" class="nav-back">← Volver</RouterLink>
      <span class="app-title">Estadísticas</span>
    </header>

    <main class="main-content">
      <div v-if="store.loading" class="state-msg">Cargando...</div>

      <div v-else-if="store.error" class="state-msg error">{{ store.error }}</div>

      <template v-else-if="store.summaries.length > 0">
        <KpiCards />
        <section class="chart-section">
          <ScoreChart :summaries="store.summaries" />
        </section>

        <section class="weeks-section">
          <div class="weeks-list">
            <div v-for="s in pastSummaries" :key="`${s.summary.year}-${s.summary.weekNumber}`">
              <WeekSummaryCard :summary="s" />
              <div v-if="!s.summary.weightKg" class="weight-form">
                <input v-model.number="weightInputs[`${s.summary.year}-${s.summary.weekNumber}`]" type="number"
                  step="0.1" placeholder="Peso (kg)" class="weight-input" />
                <button class="btn-weight"
                  @click="submitWeight(s.summary.year, s.summary.weekNumber)">Registrar</button>
              </div>
            </div>
          </div>
        </section>
      </template>

      <div v-else class="state-msg">
        Aún no hay semanas registradas.
      </div>
    </main>
  </div>
</template>

<script setup>
import { onMounted, reactive, computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useStatsStore } from '../stores/statsStore.js'
import WeekSummaryCard from '../components/WeekSummaryCard.vue'
import ScoreChart from '../components/ScoreChart.vue'
import KpiCards from '../components/KpiCards.vue'

const store = useStatsStore()
const weightInputs = reactive({})

const pastSummaries = computed(() => {
  const now = new Date()
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
  return store.summaries.filter(s => s.summary.weekEnd < today)
})

onMounted(() => store.loadStats())

async function submitWeight(year, week) {
  const key = `${year}-${week}`
  const val = weightInputs[key]
  if (!val) return
  await store.registerWeight(year, week, val)
  delete weightInputs[key]
}
</script>

<style scoped>
.stats-view {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  position: sticky;
  top: 0;
  z-index: 10;
}

.nav-back {
  color: #555;
  text-decoration: none;
  font-size: 0.9rem;
}

.app-title {
  font-weight: 700;
  font-size: 1.1rem;
}

.main-content {
  flex: 1;
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.chart-section,
.weeks-section {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.weeks-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.weight-form {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
  padding: 0 0.25rem;
}

.weight-input {
  flex: 1;
  padding: 0.4rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  font-size: 0.9rem;
}

.btn-weight {
  padding: 0.4rem 0.75rem;
  background: #1a1a1a;
  color: #fff;
  border: none;
  border-radius: 0.5rem;
  font-size: 0.85rem;
  cursor: pointer;
}

.state-msg {
  text-align: center;
  color: #888;
  margin-top: 3rem;
}

.state-msg.error {
  color: #c62828;
}
</style>
