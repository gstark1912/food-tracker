<template>
  <section class="daily-entries-list">
    <h3 class="section-title">Esta semana</h3>

    <div class="entries">
      <DailyEntryRow
        v-for="item in store.weekEntries"
        :key="item.date"
        :entry="item"
      />
    </div>

    <p v-if="store.weekEntriesError" class="error-msg">
      {{ store.weekEntriesError }}
    </p>

    <p v-if="store.weekEntriesLoading" class="loading-msg">
      Cargando...
    </p>

    <p v-if="!store.weekEntriesLoading && store.weekEntries.length === 0 && !store.weekEntriesError" class="empty-msg">
      No hay días finalizados esta semana
    </p>
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

.empty-msg {
  color: #999;
  font-size: 0.85rem;
  margin-top: 0.75rem;
  text-align: center;
}

</style>
