import { mount } from '@vue/test-utils'
import KpiCard from '../KpiCard.vue'

describe('KpiCard', () => {
    it('renderiza valor y etiqueta', () => {
        const wrapper = mount(KpiCard, {
            props: { label: 'Promedio Comida', value: 5.3 },
        })
        expect(wrapper.text()).toContain('Promedio Comida')
        expect(wrapper.text()).toContain('5.3')
    })

    it('muestra indicador de comparación con datos previos', () => {
        const wrapper = mount(KpiCard, {
            props: { label: 'Promedio Comida', value: 5.0, previousValue: 3.0 },
        })
        const comparison = wrapper.find('.kpi-comparison')
        expect(comparison.exists()).toBe(true)
        expect(comparison.text()).toContain('▲')
        expect(comparison.text()).toContain('2')
    })

    it('oculta indicador cuando previousValue es null', () => {
        const wrapper = mount(KpiCard, {
            props: { label: 'Promedio Comida', value: 5.3 },
        })
        expect(wrapper.find('.kpi-comparison').exists()).toBe(false)
    })

    it('muestra dirección correcta (sube)', () => {
        const wrapper = mount(KpiCard, {
            props: { label: 'Test', value: 6.0, previousValue: 4.0 },
        })
        const comparison = wrapper.find('.kpi-comparison')
        expect(comparison.text()).toContain('▲')
        expect(comparison.classes()).toContain('comparison-up')
    })

    it('muestra dirección correcta (baja)', () => {
        const wrapper = mount(KpiCard, {
            props: { label: 'Test', value: 3.0, previousValue: 5.0 },
        })
        const comparison = wrapper.find('.kpi-comparison')
        expect(comparison.text()).toContain('▼')
        expect(comparison.classes()).toContain('comparison-down')
    })

    it('muestra dirección correcta (igual)', () => {
        const wrapper = mount(KpiCard, {
            props: { label: 'Test', value: 5.0, previousValue: 5.0 },
        })
        const comparison = wrapper.find('.kpi-comparison')
        expect(comparison.text()).toContain('●')
        expect(comparison.classes()).toContain('comparison-equal')
    })
})
