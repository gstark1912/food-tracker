<template>
  <div class="tracker-view">
    <header class="app-header">
      <span class="app-title">🥗 Food Tracker</span>
      <RouterLink to="/stats" class="nav-link">Estadísticas</RouterLink>
    </header>

    <main class="main-content">
      <div v-if="store.loading && !store.currentEntry" class="state-msg">
        Cargando...
      </div>

      <div v-else-if="store.error" class="state-msg error">
        {{ store.error }}
        <button class="btn-retry" @click="store.init()">Reintentar</button>
      </div>

      <DayCard
        v-else-if="store.currentEntry"
        :entry="store.currentEntry"
        :loading="store.loading"
        @update:moments="onMomentsUpdate"
        @save="onSave"
        @finalize="onFinalize"
      />

      <DailyEntriesList />
    </main>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useTrackerStore } from '../stores/trackerStore.js'
import DayCard from '../components/DayCard.vue'
import DailyEntriesList from '../components/DailyEntriesList.vue'

const store = useTrackerStore()

onMounted(() => {
  store.init()
  store.loadCurrentWeekEntries()
})

function onMomentsUpdate(moments) {
  if (store.currentEntry) {
    store.currentEntry.moments = moments
  }
}

async function onSave() {
  await store.saveCurrentDay()
}

async function onFinalize() {
  if (store.currentEntry) {
    await store.finalizeDay(store.currentEntry.date)
  }
}
</script>

<style scoped>
.tracker-view {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}
.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  position: sticky;
  top: 0;
  z-index: 10;
}
.app-title {
  font-weight: 700;
  font-size: 1.1rem;
}
.nav-link {
  color: #555;
  text-decoration: none;
  font-size: 0.9rem;
}
.main-content {
  flex: 1;
  padding: 1.25rem;
}
.state-msg {
  text-align: center;
  color: #888;
  margin-top: 3rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}
.state-msg.error {
  color: #c62828;
}
.btn-retry {
  padding: 0.5rem 1rem;
  border: 1px solid #c62828;
  border-radius: 0.5rem;
  background: transparent;
  color: #c62828;
  cursor: pointer;
}
</style>
