export interface Customer {
  id: number;
  address: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
}
export interface UpdateCustomerRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  address: string;
}