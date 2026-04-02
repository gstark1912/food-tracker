import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import { createPinia, setActivePinia } from 'pinia'

const mockKpis = ref(null)
const mockKpisLoading = ref(false)
const mockKpisError = ref(null)
const mockLoadKpis = vi.fn()

vi.mock('../../stores/statsStore.js', () => ({
    useStatsStore: () => ({
        kpis: mockKpis.value,
        kpisLoading: mockKpisLoading.value,
        kpisError: mockKpisError.value,
        loadKpis: mockLoadKpis,
    }),
}))

import KpiCards from '../KpiCards.vue'

describe('KpiCards', () => {
    beforeEach(() => {
        setActivePinia(createPinia())
        mockKpis.value = null
        mockKpisLoading.value = false
        mockKpisError.value = null
        mockLoadKpis.mockClear()
    })

    it('muestra indicador de carga', () => {
        mockKpisLoading.value = true
        const wrapper = mount(KpiCards)
        expect(wrapper.text()).toContain('Cargando KPIs...')
    })

    it('muestra mensaje de error', () => {
        mockKpisError.value = 'Error al cargar KPIs'
        const wrapper = mount(KpiCards)
        expect(wrapper.text()).toContain('Error al cargar KPIs')
    })

    it('renderiza tarjetas con datos', () => {
        mockKpis.value = {
            currentWeek: { year: 2025, weekNumber: 27, avgFood: 5.3, totalExercise: 18, finalizedDays: 4 },
            previousWeek: { year: 2025, weekNumber: 26, avgFood: 6.1, totalExercise: 12, finalizedDays: 7 },
        }
        const wrapper = mount(KpiCards)
        const cards = wrapper.findAllComponents({ name: 'KpiCard' })
        expect(cards).toHaveLength(2)
        expect(wrapper.text()).toContain('5.3')
        expect(wrapper.text()).toContain('18')
    })
})
