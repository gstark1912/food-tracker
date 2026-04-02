<template>
  <section class="daily-entries-list">
    <h3 class="section-title">Historial</h3>

    <div class="entries">
      <DailyEntryRow
        v-for="item in store.dailyEntries"
        :key="item.date"
        :entry="item"
      />
    </div>

    <p v-if="store.dailyEntriesError" class="error-msg">
      {{ store.dailyEntriesError }}
    </p>

    <p v-if="store.dailyEntriesLoading" class="loading-msg">
      Cargando...
    </p>

    <button
      v-if="store.dailyEntriesHasMore"
      class="btn btn-load-more"
      :disabled="store.dailyEntriesLoading"
      @click="store.loadMoreEntries()"
    >
      Cargar más
    </button>
  </section>
</template>

<script setup>
import { useTrackerStore } from '../stores/trackerStore.js'
import DailyEntryRow from './DailyEntryRow.vue'

const store = useTrackerStore()
</script>

<style scoped>
.daily-entries-list {
  max-width: 480px;
  margin: 1.5rem auto 0;
}

.section-title {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: #1a1a1a;
}

.entries {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.error-msg {
  color: #c62828;
  font-size: 0.85rem;
  margin-top: 0.75rem;
  text-align: center;
}

.loading-msg {
  color: #777;
  font-size: 0.85rem;
  margin-top: 0.75rem;
  text-align: center;
}

.btn-load-more {
  display: block;
  width: 100%;
  margin-top: 0.75rem;
  padding: 0.6rem 1.25rem;
  border: none;
  border-radius: 0.5rem;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  background: #f0f0f0;
  color: #1a1a1a;
  transition: opacity 0.15s;
}

.btn-load-more:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
