<template>
  <div class="moment-row">
    <span class="moment-name">{{ momentEntry.moment }}</span>
    <div class="moment-fields">
      <div class="field">
        <span class="field-label">🍽</span>
        <FibonacciControl
          :value="momentEntry.food"
          :disabled="readonly"
          @increment="emit('update:momentEntry', { ...momentEntry, food: fibNext(momentEntry.food) })"
          @decrement="emit('update:momentEntry', { ...momentEntry, food: fibPrev(momentEntry.food) })"
        />
      </div>
      <div class="field">
        <span class="field-label">🏃</span>
        <FibonacciControl
          :value="momentEntry.exercise"
          :disabled="readonly"
          @increment="emit('update:momentEntry', { ...momentEntry, exercise: fibNext(momentEntry.exercise) })"
          @decrement="emit('update:momentEntry', { ...momentEntry, exercise: fibPrev(momentEntry.exercise) })"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import FibonacciControl from './FibonacciControl.vue'

const FIBONACCI = [0, 1, 2, 3, 5, 8, 13]
const fibNext = v => { const i = FIBONACCI.indexOf(v); return i >= 0 && i < FIBONACCI.length - 1 ? FIBONACCI[i + 1] : v }
const fibPrev = v => { const i = FIBONACCI.indexOf(v); return i > 0 ? FIBONACCI[i - 1] : 0 }

defineProps({
  momentEntry: { type: Object, required: true },
  readonly: { type: Boolean, default: false },
})
const emit = defineEmits(['update:momentEntry'])
</script>

<style scoped>
.moment-row {
  display: flex;
  flex-direction: column;
  padding: 0.75rem 0;
  border-bottom: 1px solid #f0f0f0;
  gap: 0.5rem;
}
.moment-row:last-child {
  border-bottom: none;
}
.moment-name {
  font-weight: 500;
  color: #555;
}
.moment-fields {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.field {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.4rem;
}
.field-label {
  font-size: 1rem;
}
</style>
