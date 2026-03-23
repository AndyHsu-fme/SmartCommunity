import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import Layout from "./components/Layout";
import LoginPage from "./pages/LoginPage";
import VotePage from "./pages/VotePage";
import ReservationPage from "./pages/ReservationPage";
import PackagePage from "./pages/PackagePage";
import PaymentPage from "./pages/PaymentPage";
import AdminPage from "./pages/AdminPage";

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<Layout />}>
            <Route index element={<Navigate to="/vote" replace />} />
            <Route path="vote" element={<VotePage />} />
            <Route path="reservation" element={<ReservationPage />} />
            <Route path="package" element={<PackagePage />} />
            <Route path="payment" element={<PaymentPage />} />
            <Route path="admin" element={<AdminPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
