using System;
using System.Collections.Generic;
using System.Text;

namespace test.steps
{
    public class ClientMocker
    {
        private static OkHttpClient mockClient = Mockito.mock(OkHttpClient.class);

    public static void infect(Client client) 
        {
            // Infect the client with a mock, assign it to the private field.
            FieldUtils.writeField(client, "client", mockClient, true);
    }

    /**
     * Orchestrates a mocked response to be returned by the low level client the next time 'execute()' is called.
     */
    public static void oneResponse(int code, String contentType, File bodyFile)
    {
        Mockito.reset(mockClient);

        byte[]
        bytes = Files.readAllBytes(bodyFile.toPath());
        if (bodyFile.getName().endsWith("base64")) {
            bytes = Encoder.decodeFromBase64(new String(bytes));
        }

        // Give the lambda a final variable.
        final byte[]
        bodyBytes = bytes;

        // Return a mock "Call" which returns a mock "Response" loaded with the mock body data and return code.
        Mockito.when(mockClient.newCall(Mockito.any())).thenAnswer(i -> {
            Request r = i.getArgument(0);

            ResponseBody body = ResponseBody.create(
                    MediaType.parse(contentType),
                    bodyBytes);

            Response mockResponse = (new Response.Builder())
                    .code(code)
                    .body(body)
                    .request(r)
                    .protocol(Protocol.HTTP_2)
                    .build();

            Call c = Mockito.mock(Call.class);
            Mockito.when(c.execute()).thenReturn(mockResponse);

            return c;
        });
    }
    }
}
