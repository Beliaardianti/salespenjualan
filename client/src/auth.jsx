import { createContext, useContext, useEffect, useState } from 'react'
import { api, getToken, setToken, setUnauthorizedHandler } from './api'

// Context = "variabel global" React untuk data user yang sedang login
const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(!!getToken())

  const logout = () => {
    setToken(null)
    setUser(null)
  }

  useEffect(() => {
    setUnauthorizedHandler(logout)
    // Kalau masih ada token dari sesi sebelumnya, ambil data user-nya
    if (getToken()) {
      api.get('/auth/me')
        .then(setUser)
        .catch(logout)
        .finally(() => setLoading(false))
    }
  }, [])

  const login = async (username, password) => {
    const res = await api.post('/auth/login', { username, password })
    setToken(res.token)
    setUser(res.user)
  }

  const isAdmin = user?.role === 'Admin'

  return (
    <AuthContext.Provider value={{ user, isAdmin, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => useContext(AuthContext)
