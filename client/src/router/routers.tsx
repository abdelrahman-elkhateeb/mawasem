export const routes = [
  {
    path: "/",
    Component: RootLayout,
    children: [
      {index: true, Component: HomePage}
    ]
  }
]