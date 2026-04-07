import { defineStore } from 'pinia'
import { ref } from 'vue'

const FIBONACCI = [0, 1, 2, 3, 5, 8, 13]

function fibNext(val) {
    const idx = FIBONACCI.indexOf(val)
    return idx >= 0 && idx < FIBONACCI.length - 1 ? FIBONACCI[idx + 1] : val
}

function fibPrev(val) {
    const idx = FIBONACCI.indexOf(val)
    return idx > 0 ? FIBONACCI[idx - 1] : 0
}

export const useTrackerStore = defineStore('tracker', () => {
    const currentEntry = ref(null)
    const loading = ref(false)
    const error = ref(null)

    async function init() {
        loading.value = true
        error.value = null
        try {
            const res = await fetch('/api/tracker/next-pending')
            if (!res.ok) throw new Error('Error al cargar el día')
            currentEntry.value = await res.json()
        } catch (e) {
            error.value = e.message
        } finally {
            loading.value = false
        }
    }

    async function saveCurrentDay() {
        if (!currentEntry.value) return
        loading.value = true
        error.value = null
        try {
            const res = await fetch(`/api/tracker/day/${currentEntry.value.date}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ moments: currentEntry.value.moments }),
            })
            if (!res.ok) {
                const data = await res.json()
                throw new Error(data.error || 'Error al guardar')
            }
        } catch (e) {
            error.value = e.message
        } finally {
            loading.value = false
        }
    }

    async function finalizeDay(date) {
        loading.value = true
        error.value = null
        try {
            const res = await fetch(`/api/tracker/day/${date}/finalize`, { method: 'POST' })
            if (!res.ok) {
                const data = await res.json()
                throw new Error(data.error || 'Error al finalizar')
            }
            const data = await res.json()
            if (data.nextPendingDate) {
                await init()
            }
        } catch (e) {
            error.value = e.message
        } finally {
            loading.value = false
        }
    }

    function incrementMoment(momentName, field) {
        if (!currentEntry.value) return
        const moment = currentEntry.value.moments.find(m => m.moment === momentName)
        if (moment) moment[field] = fibNext(moment[field])
    }

    function decrementMoment(momentName, field) {
        if (!currentEntry.value) return
        const moment = currentEntry.value.moments.find(m => m.moment === momentName)
        if (moment) moment[field] = fibPrev(moment[field])
    }

    const weekEntries = ref([])
    const weekEntriesLoading = ref(false)
    const weekEntriesError = ref(null)

    async function loadCurrentWeekEntries() {
        weekEntriesLoading.value = true
        weekEntriesError.value = null
        try {
            const res = await fetch('/api/tracker/days/current-week')
            if (!res.ok) throw new Error('Error al cargar la semana')
            weekEntries.value = await res.json()
        } catch (e) {
            weekEntriesError.value = e.message
        } finally {
            weekEntriesLoading.value = false
        }
    }

    const dailyEntries = ref([])
    const dailyEntriesLoading = ref(false)
    const dailyEntriesError = ref(null)
    const dailyEntriesPage = ref(1)
    const dailyEntriesHasMore = ref(false)

    async function loadDailyEntries(page = 1) {
        dailyEntriesLoading.value = true
        dailyEntriesError.value = null
        try {
            const res = await fetch(`/api/tracker/days?page=${page}&pageSize=10`)
            if (!res.ok) throw new Error('Error al cargar historial')
            const data = await res.json()

            if (page === 1) {
                dailyEntries.value = data.items
            } else {
                dailyEntries.value = [...dailyEntries.value, ...data.items]
            }
            dailyEntriesPage.value = page
            dailyEntriesHasMore.value = data.hasMore
        } catch (e) {
            dailyEntriesError.value = e.message
        } finally {
            dailyEntriesLoading.value = false
        }
    }

    function loadMoreEntries() {
        loadDailyEntries(dailyEntriesPage.value + 1)
    }

    return { currentEntry, loading, error, init, saveCurrentDay, finalizeDay, incrementMoment, decrementMoment, weekEntries, weekEntriesLoading, weekEntriesError, loadCurrentWeekEntries, dailyEntries, dailyEntriesLoading, dailyEntriesError, dailyEntriesPage, dailyEntriesHasMore, loadDailyEntries, loadMoreEntries }
})
