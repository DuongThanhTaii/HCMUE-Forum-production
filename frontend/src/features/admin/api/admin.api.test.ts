import { describe, expect, it } from 'vitest'
import { normalizeScopeValue, unwrapApiData, unwrapApiList } from './admin.api'

describe('admin.api envelope helpers', () => {
  it('unwraps { success, data } envelope for roles list', () => {
    const response = {
      success: true,
      data: [{ id: '1', name: 'Admin', description: '', isDefault: false, isSystemRole: true, permissionCount: 0, createdAt: '' }],
    }
    const list = unwrapApiList<typeof response.data[0]>(response)
    expect(list).toHaveLength(1)
    expect(list[0]?.name).toBe('Admin')
  })

  it('unwraps Pascal-case Data envelope', () => {
    const response = { Success: true, Data: [{ id: 'x' }] }
    expect(unwrapApiList<{ id: string }>(response)).toEqual([{ id: 'x' }])
  })

  it('accepts raw array response', () => {
    const response = [{ id: 'a' }, { id: 'b' }]
    expect(unwrapApiList<{ id: string }>(response)).toEqual(response)
  })

  it('unwrapApiData returns nested object', () => {
    const response = { success: true, data: { id: 'r1', name: 'Mod' } }
    expect(unwrapApiData<{ id: string; name: string }>(response)).toEqual({
      id: 'r1',
      name: 'Mod',
    })
  })
})

describe('normalizeScopeValue', () => {
  it('normalizes null scope to empty string for query params', () => {
    expect(normalizeScopeValue(null)).toBe('')
  })

  it('preserves non-null scope values', () => {
    expect(normalizeScopeValue('faculty:it')).toBe('faculty:it')
  })
})
