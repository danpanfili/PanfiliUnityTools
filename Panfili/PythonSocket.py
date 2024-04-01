import socket
import mat2json

# Define host and port
HOST = '127.0.0.1'  # localhost
PORT = 12345
BUFFER = pow(1024,1)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket: # Create a TCP/IP socket
    server_socket.bind((HOST, PORT)) # Bind the socket to the address

    server_socket.listen() # Listen for incoming connections
    print('Server is listening...')

    connection, address = server_socket.accept() # Accept incoming connection
    print('Connected by', address)

    with connection:
        while True:
            # Receive data from the client
            data = connection.recv(BUFFER)
            if not data:
                break
            request = data.decode()
            print(f'Received: {request}')
            
            response = mat2json.SocketRequest(request)
            connection.sendall(response) # Send a response back to the client