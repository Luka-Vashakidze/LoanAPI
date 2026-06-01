import { useNavigate, Link } from "react-router-dom";
import api from "../api/client";
import LoanForm from "../components/LoanForm";

export default function CreateLoan() {
  const navigate = useNavigate();

  return (
    <div>
      <Link to="/loans" className="text-sm text-muted hover:text-ink">← Back to loans</Link>
      <h1 className="font-serif text-4xl mt-4 mb-8">New loan</h1>
      <LoanForm
        submitLabel="Submit application"
        onSubmit={async (values) => {
          await api.post("/loan", values);
          navigate("/loans");
        }}
      />
    </div>
  );
}