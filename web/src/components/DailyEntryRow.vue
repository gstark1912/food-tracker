<template>
  <div class="daily-entry-row">
    <span class="entry-date">{{ formattedDate }}</span>
    <span class="entry-cell">🌅 {{ entry.foodMañana }}</span>
    <span class="entry-cell">🌞 {{ entry.foodMediodia }}</span>
    <span class="entry-cell">🌇 {{ entry.foodTarde }}</span>
    <span class="entry-cell">🌙 {{ entry.foodNoche }}</span>
    <span class="entry-cell entry-exercise">🏃 {{ entry.totalExercise }}</span>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  entry: { type: Object, required: true },
})

const formattedDate = computed(() => {
  const [year, month, day] = props.entry.date.split('-')
  const date = new Date(Number(year), Number(month) - 1, Number(day))
  return date.toLocaleDateString('es-AR', { day: 'numeric', month: 'short' })
})
</script>

<style scoped>
.daily-entry-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 0.75rem;
  background: #fff;
  border-radius: 0.75rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.entry-date {
  font-weight: 600;
  font-size: 0.85rem;
  color: #333;
  min-width: 3.5rem;
  text-transform: lowercase;
}

.entry-cell {
  flex: 1;
  text-align: center;
  font-size: 0.85rem;
  color: #555;
}

.entry-exercise {
  font-weight: 600;
  color: #1a1a1a;
}
</style>
