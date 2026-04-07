<template>
    <div class="kpi-cards-section">
        <div v-if="statsStore.kpisLoading" class="kpi-state">Cargando KPIs...</div>
        <div v-else-if="statsStore.kpisError" class="kpi-state kpi-error">{{ statsStore.kpisError }}</div>
        <div v-else-if="statsStore.kpis" class="kpi-row">
            <KpiCard label="Promedio Comida" :value="statsStore.kpis.currentWeek.avgFood"
                :previousValue="statsStore.kpis.previousWeek?.avgFood ?? null" :lowerIsBetter="true" />
            <KpiCard label="Total Ejercicio" :value="statsStore.kpis.currentWeek.totalExercise"
                :previousValue="statsStore.kpis.previousWeek?.totalExercise ?? null" :lowerIsBetter="false" />
        </div>
    </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useStatsStore } from '../stores/statsStore.js'
import KpiCard from './KpiCard.vue'

const statsStore = useStatsStore()

onMounted(() => {
    statsStore.loadKpis()
})
</script>

<style scoped>
.kpi-cards-section {
    width: 100%;
}

.kpi-row {
    display: flex;
    gap: 0.75rem;
}

.kpi-state {
    text-align: center;
    color: #888;
    padding: 1rem;
}

.kpi-error {
    color: #c62828;
}

@media (max-width: 480px) {
    .kpi-row {
        flex-direction: column;
    }
}
</style>
