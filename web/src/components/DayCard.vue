<template>
  <div class="day-card">
    <div class="day-header">
      <h2 class="day-date">{{ formattedDate }}</h2>
      <span v-if="entry.isCurrentDay" class="badge-today">Hoy</span>
      <span v-if="entry.isFinalized" class="badge-done">✓ Finalizado</span>
    </div>

    <div class="moments">
      <MomentRow
        v-for="m in localMoments"
        :key="m.moment"
        :momentEntry="m"
        :readonly="entry.isFinalized"
        @update:momentEntry="updateMoment"
      />
    </div>

    <div v-if="!entry.isFinalized" class="actions">
      <button
        v-if="entry.isCurrentDay"
        class="btn btn-secondary"
        :disabled="loading"
        @click="$emit('save')"
      >
        {{ loading ? 'Guardando...' : 'Guardar' }}
      </button>
      <button
        class="btn btn-primary"
        :disabled="loading"
        @click="$emit('finalize')"
      >
        {{ loading ? 'Finalizando...' : 'Finalizar día' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import MomentRow from './MomentRow.vue'

const props = defineProps({
  entry: { type: Object, required: true },
  loading: { type: Boolean, default: false },
})
const emit = defineEmits(['save', 'finalize', 'update:moments'])

const localMoments = ref(props.entry.moments.map(m => ({ ...m })))

watch(() => props.entry, (newEntry) => {
  localMoments.value = newEntry.moments.map(m => ({ ...m }))
}, { deep: true })

function updateMoment(updated) {
  const idx = localMoments.value.findIndex(m => m.moment === updated.moment)
  if (idx >= 0) {
    localMoments.value[idx] = updated
    emit('update:moments', localMoments.value)
  }
}

const formattedDate = computed(() => {
  const [year, month, day] = props.entry.date.split('-')
  const date = new Date(Number(year), Number(month) - 1, Number(day))
  return date.toLocaleDateString('es-AR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
})
</script>

<style scoped>
.day-card {
  background: #fff;
  border-radius: 1rem;
  padding: 1.25rem;
  box-shadow: 0 1px 4px rgba(0,0,0,0.08);
  max-width: 480px;
  margin: 0 auto;
}
.day-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}
.day-date {
  font-size: 1rem;
  font-weight: 600;
  text-transform: capitalize;
  flex: 1;
}
.badge-today {
  background: #e8f5e9;
  color: #2e7d32;
  padding: 0.2rem 0.6rem;
  border-radius: 1rem;
  font-size: 0.75rem;
  font-weight: 600;
}
.badge-done {
  background: #e3f2fd;
  color: #1565c0;
  padding: 0.2rem 0.6rem;
  border-radius: 1rem;
  font-size: 0.75rem;
  font-weight: 600;
}
.moments {
  margin-bottom: 1rem;
}
.actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
}
.btn {
  padding: 0.6rem 1.25rem;
  border: none;
  border-radius: 0.5rem;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn-primary {
  background: #1a1a1a;
  color: #fff;
}
.btn-secondary {
  background: #f0f0f0;
  color: #1a1a1a;
}
</style>
