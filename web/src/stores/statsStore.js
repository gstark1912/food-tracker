import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useStatsStore = defineStore('stats', () => {
    const summaries = ref([])
    const loading = ref(false)
    const error = ref(null)

    async function loadStats() {
        loading.value = true
        error.value = null
        try {
            const res = await fetch('/api/statistics')
            if (!res.ok) throw new Error('Error al cargar estadísticas')
            const data = await res.json()
            summaries.value = data.summaries
        } catch (e) {
            error.value = e.message
        } finally {
            loading.value = false
        }
    }

    async function registerWeight(year, week, weightKg) {
        error.value = null
        try {
            const res = await fetch(`/api/weekly/${year}/${week}/weight`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ weightKg }),
            })
            if (!res.ok) {
                const data = await res.json()
                throw new Error(data.error || 'Error al registrar peso')
            }
            await loadStats()
        } catch (e) {
            error.value = e.message
        }
    }

    return { summaries, loading, error, loadStats, registerWeight }
})
