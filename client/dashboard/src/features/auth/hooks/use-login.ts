
import { useMutation, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { useState } from "react";
import { login } from "../api/login";

export function useLogin() {
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const { isPending: isUserLoading, mutate: loginUser } = useMutation({
    mutationFn: login,

    onMutate: () => {
      setErrorMessage(null);
    },

    onError: (error) => {
      let message = "Something went wrong. Please try again.";

      if (axios.isAxiosError(error)) {
        switch (error.response?.status) {
          case 401:
            message = "Invalid credentials";
            break;
          case 403:
            message = "You are not authorized to access this dashboard";
            break;
          case 423:
            message = "Your account is temporarily locked";
            break;
          default:
            message = "Something went wrong. Please try again.";
            break;
        }
      }

      setErrorMessage(message);
    },

    onSuccess: async () => {
      setErrorMessage(null);

      await queryClient.invalidateQueries({
        queryKey: ["me"],
      });
    }
  });

  return { isUserLoading, loginUser, errorMessage }
}